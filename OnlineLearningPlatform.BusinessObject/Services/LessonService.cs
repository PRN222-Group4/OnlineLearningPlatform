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
                var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == request.ModuleId);
                if (module == null) return response.SetNotFound("Module not found!");

                var existingLessons = await _unitOfWork.Lessons.GetAllAsync(l => l.ModuleId == request.ModuleId && !l.IsDeleted);
                int newOrderIndex = existingLessons.Any() ? existingLessons.Max(l => l.OrderIndex) + 1 : 1;

                var lesson = _mapper.Map<Lesson>(request);
                lesson.CreatedBy = claim.UserId;
                lesson.OrderIndex = newOrderIndex;

                lesson.LessonItems = new List<LessonItem>();

                await _unitOfWork.Lessons.AddAsync(lesson);

                await _unitOfWork.SaveChangeAsync();

                // Lúc này lesson.GradedItems đã có dữ liệu, Mapper sẽ hoạt động đúng
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
                var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId);
                if (lesson == null)
                    return response.SetNotFound("Lesson not found");

                _mapper.Map(request, lesson);
                lesson.UpdatedBy = _service.GetUserClaim().UserId;

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
                var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId && !l.IsDeleted);
                if (lesson == null)
                    return response.SetNotFound("Lesson not found");

                // Soft delete instead of hard delete to avoid FK constraint with UserLessonProgress
                lesson.IsDeleted = true;
                lesson.UpdatedBy = _service.GetUserClaim().UserId;
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
    }
}