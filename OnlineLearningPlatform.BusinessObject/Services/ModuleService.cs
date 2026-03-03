using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Module;
using OnlineLearningPlatform.BusinessObject.Responses.Lesson;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _service;

        public ModuleService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
        }

        public async Task<ApiResponse> CreateNewModuleForCourseAsync(CreateNewModuleForCourseRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == request.CourseId);
                if (course == null)
                {
                    return response.SetNotFound(message: "Course not found or may have been automatically deleted due to inactivity!!!");
                }

                var existingModules = await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == request.CourseId && !m.IsDeleted);
                int newIndex = existingModules.Any() ? existingModules.Max(m => m.Index) + 1 : 1;

                var module = _mapper.Map<Module>(request);
                module.CreatedBy = _service.GetUserClaim().UserId;
                module.Index = newIndex;

                await _unitOfWork.Modules.AddAsync(module);
                await _unitOfWork.SaveChangeAsync();

                var result = _mapper.Map<ModuleResponse>(module);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteModuleAsync(Guid moduleId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == moduleId && !m.IsDeleted);

                if (module == null)
                    return response.SetNotFound("Module not found");

                module.IsDeleted = true;
                module.UpdatedAt = DateTime.UtcNow;
                module.UpdatedBy = _service.GetUserClaim().UserId;

                _unitOfWork.Modules.Update(module);
                await _unitOfWork.SaveChangeAsync();

                return response.SetOk("Module deleted successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetModuleDetailAsync(Guid moduleId)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == moduleId && !m.IsDeleted);

                if (module == null)
                    return response.SetNotFound("Module not found");

                var result = _mapper.Map<ModuleResponse>(module);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetModulesByCourseAsync(Guid courseId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var modules = await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == courseId && !m.IsDeleted);

                var moduleResponses = _mapper.Map<List<ModuleResponse>>(modules);

                foreach (var mod in moduleResponses)
                {
                    
                    var lessons = await _unitOfWork.Lessons.GetAllAsync(
                        filter: l => l.ModuleId == mod.ModuleId && !l.IsDeleted,
                        include: q => q.Include(l => l.LessonItems)
                    );

                    mod.Lessons = _mapper.Map<List<LessonResponse>>(lessons)
                                         .OrderBy(l => l.OrderIndex).ToList();
                }

                return response.SetOk(moduleResponses.OrderBy(m => m.Index).ToList());
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateModuleAsync(UpdateModuleRequest request)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == request.ModuleId && !m.IsDeleted);

                if (module == null)
                    return response.SetNotFound("Module not found");

                _mapper.Map(request, module);
                module.UpdatedAt = DateTime.UtcNow;
                module.UpdatedBy = _service.GetUserClaim().UserId;

                _unitOfWork.Modules.Update(module);
                await _unitOfWork.SaveChangeAsync();

                return response.SetOk("Module updated successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}