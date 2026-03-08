using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.Lesson;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Lesson;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _service;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
        }

        public async Task<ApiResponse> CreateNewLessonForModuleAsync(CreateNewLessonForModuleRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == request.ModuleId && !m.IsDeleted);
                if (module == null) return response.SetNotFound("Module not found!");
                var verifyResult = await VerifyCanEditModuleAsync(module, claim.UserId);
                if (verifyResult != null) return verifyResult;

                var existingLessons = await _unitOfWork.Lessons.GetAllAsync(l => l.ModuleId == request.ModuleId && !l.IsDeleted);
                int newOrderIndex = existingLessons.Any() ? existingLessons.Max(l => l.OrderIndex) + 1 : 1;

                var lesson = _mapper.Map<Lesson>(request);
                lesson.LessonId = Guid.NewGuid();
                lesson.CreatedBy = claim.UserId;
                lesson.OrderIndex = newOrderIndex;

                lesson.LessonItems = new List<LessonItem>();

                await _unitOfWork.Lessons.AddAsync(lesson);

                await _unitOfWork.SaveChangeAsync();

                var result = _mapper.Map<LessonResponse>(lesson);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId && !l.IsDeleted);
                if (lesson == null)
                    return response.SetNotFound("Lesson not found");
                var claim = _service.GetUserClaim();
                var verifyResult = await VerifyCanEditLessonAsync(lesson, claim.UserId);
                if (verifyResult != null) return verifyResult;

                _mapper.Map(request, lesson);
                lesson.UpdatedBy = claim.UserId;

                await _unitOfWork.SaveChangeAsync();
                return response.SetOk("Lesson updated successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteLessonAsync(Guid lessonId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId && !l.IsDeleted);
                if (lesson == null)
                    return response.SetNotFound("Lesson not found");
                var verifyResult = await VerifyCanEditLessonAsync(lesson, claim.UserId);
                if (verifyResult != null) return verifyResult;

                // Soft delete instead of hard delete to avoid FK constraint with UserLessonProgress
                lesson.IsDeleted = true;
                lesson.UpdatedBy = claim.UserId;
                await _unitOfWork.SaveChangeAsync();

                return response.SetOk("Lesson deleted successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetLessonsByModuleAsync(Guid moduleId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var lessons = await _unitOfWork.Lessons.GetAllAsync(
                    l => l.ModuleId == moduleId
                );

                lessons = lessons.OrderBy(l => l.OrderIndex).ToList();
                var lessonItems = await _unitOfWork.LessonItems.GetAllAsync(li => lessons.Select(l => l.LessonId).Contains(li.LessonId));
                var result = new
                {
                    Total = lessons.Count(),
                    VideoCount = lessonItems.Count(l => l.Type == 0),
                    ReadingCount = lessonItems.Count(l => l.Type == 1),
                    PracticeCount = lessonItems.Count(l => l.Type == 5),
                    GradedCount = lessonItems.Count(l => l.Type == 7),
                    Lessons = _mapper.Map<List<LessonResponse>>(lessons)
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetLessonDetailAsync(Guid lessonId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var lesson = await _unitOfWork.Lessons.GetAsync(
                    l => l.LessonId == lessonId);

                if (lesson == null)
                    return response.SetNotFound("Lesson not found");

                var result = _mapper.Map<LessonDetailResponse>(lesson);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        private async Task<ApiResponse?> VerifyCanEditModuleAsync(Module module, Guid userId)
        {
            var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == module.CourseId && !c.IsDeleted);
            if (course == null) return new ApiResponse().SetNotFound("Course not found");
            if (course.CreatedBy != userId) return new ApiResponse().SetBadRequest(message: "Bạn không có quyền chỉnh sửa khóa học này");
            if (course.Status != 0) return new ApiResponse().SetBadRequest(message: "Chỉ có thể chỉnh sửa khóa học ở trạng thái Draft");
            return null;
        }

        private async Task<ApiResponse?> VerifyCanEditLessonAsync(Lesson lesson, Guid userId)
        {
            var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == lesson.ModuleId && !m.IsDeleted);
            if (module == null) return new ApiResponse().SetNotFound("Module not found");
            return await VerifyCanEditModuleAsync(module, userId);
        }
    }
}
