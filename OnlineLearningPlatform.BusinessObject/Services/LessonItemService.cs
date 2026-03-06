using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.LessonItem;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class LessonItemService : ILessonItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly ILogger<LessonItemService> _logger;

        public LessonItemService(
            IUnitOfWork unitOfWork,
            IClaimService claimService,
            IFirebaseStorageService firebaseStorageService,
            ILogger<LessonItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _firebaseStorageService = firebaseStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse> GetLessonItemsByLessonAsync(Guid lessonId)
        {
            var response = new ApiResponse();
            try
            {
                var items = await _unitOfWork.LessonItems.GetAllAsync(li => li.LessonId == lessonId && !li.IsDeleted);
                var itemsList = items.OrderBy(li => li.OrderIndex).ToList();

                var itemIds = itemsList.Select(li => li.LessonItemId).ToList();
                var resources = await _unitOfWork.LessonResources.GetAllAsync(lr => itemIds.Contains(lr.LessonItemId) && !lr.IsDeleted);
                var gradedItems = await _unitOfWork.GradedItems.GetAllAsync(gi => itemIds.Contains(gi.LessonItemId) && !gi.IsDeleted);
                var gradedItemIds = gradedItems.Select(gi => gi.GradedItemId).ToList();
                var questions = (await _unitOfWork.Questions.GetAllAsync(q => gradedItemIds.Contains(q.GradedItemId) && !q.IsDeleted))
                    .OrderBy(q => q.OrderIndex).ToList();
                var questionIds = questions.Select(q => q.QuestionId).ToList();
                var answerOptions = (await _unitOfWork.AnswerOptions.GetAllAsync(ao => questionIds.Contains(ao.QuestionId) && !ao.IsDeleted))
                    .OrderBy(ao => ao.OrderIndex).ToList();

                var result = new
                {
                    LessonItems = itemsList,
                    LessonResources = resources,
                    GradedItems = gradedItems,
                    Questions = questions,
                    AnswerOptions = answerOptions
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson items for lesson {LessonId}", lessonId);
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetLessonItemDetailAsync(Guid lessonItemId)
        {
            var response = new ApiResponse();
            try
            {
                var item = await _unitOfWork.LessonItems.GetAsync(li => li.LessonItemId == lessonItemId && !li.IsDeleted);
                if (item == null) return response.SetNotFound(message: "Không tìm thấy tài liệu");

                var resources = await _unitOfWork.LessonResources.GetAllAsync(lr => lr.LessonItemId == lessonItemId && !lr.IsDeleted);
                GradedItem? gradedItem = null;
                List<Question> questions = new();
                List<AnswerOption> answerOptions = new();

                if (item.Type == 2) // Quiz
                {
                    gradedItem = await _unitOfWork.GradedItems.GetAsync(gi => gi.LessonItemId == lessonItemId && !gi.IsDeleted);
                    if (gradedItem != null)
                    {
                        questions = (await _unitOfWork.Questions.GetAllAsync(q => q.GradedItemId == gradedItem.GradedItemId && !q.IsDeleted))
                            .OrderBy(q => q.OrderIndex).ToList();
                        var qIds = questions.Select(q => q.QuestionId).ToList();
                        answerOptions = (await _unitOfWork.AnswerOptions.GetAllAsync(ao => qIds.Contains(ao.QuestionId) && !ao.IsDeleted))
                            .OrderBy(ao => ao.OrderIndex).ToList();
                    }
                }

                return response.SetOk(new
                {
                    LessonItem = item,
                    Resources = resources.ToList(),
                    GradedItem = gradedItem,
                    Questions = questions,
                    AnswerOptions = answerOptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson item detail {LessonItemId}", lessonItemId);
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> CreateReadingItemAsync(CreateReadingItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();

                // Verify lesson exists and course is Draft
                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                await _unitOfWork.BeginTransactionAsync();

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 1, // Reading
                    OrderIndex = request.OrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                var resource = new LessonResource
                {
                    LessonResourceId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    Title = request.Title,
                    ResourceType = 0, // Text/Reading
                    TextContent = request.Content,
                    OrderIndex = 0,
                    VideoSourceType = 0,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonResources.AddAsync(resource);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Created Reading item {ItemId} for lesson {LessonId}", lessonItem.LessonItemId, request.LessonId);
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating reading item for lesson {LessonId}", request.LessonId);
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> CreateVideoItemAsync(CreateVideoItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();

                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                // Validate video source
                string? videoUrl = null;
                if (request.VideoSourceType == 1) // YouTube
                {
                    if (string.IsNullOrWhiteSpace(request.VideoUrl))
                        return response.SetBadRequest(message: "YouTube URL là bắt buộc");
                    if (!IsValidYouTubeUrl(request.VideoUrl))
                        return response.SetBadRequest(message: "URL YouTube không hợp lệ");
                    videoUrl = request.VideoUrl;
                }
                else if (request.VideoSourceType == 2) // Mp4 Upload
                {
                    if (request.VideoFile == null)
                        return response.SetBadRequest(message: "File video Mp4 là bắt buộc");
                    var uploadResult = await _firebaseStorageService.UploadLessonResourceAsync(request.LessonId, "video", request.VideoFile);
                    videoUrl = uploadResult.Url;
                }
                else
                {
                    return response.SetBadRequest(message: "Loại video không hợp lệ");
                }

                await _unitOfWork.BeginTransactionAsync();

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 0, // Video
                    OrderIndex = request.OrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                var resource = new LessonResource
                {
                    LessonResourceId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    Title = request.Title,
                    ResourceType = 1, // Video
                    ResourceUrl = videoUrl,
                    VideoSourceType = request.VideoSourceType,
                    OrderIndex = 0,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonResources.AddAsync(resource);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Created Video item {ItemId} for lesson {LessonId}", lessonItem.LessonItemId, request.LessonId);
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating video item for lesson {LessonId}", request.LessonId);
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> CreateQuizItemAsync(CreateQuizItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();

                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                // Validate quiz structure
                if (request.Questions == null || request.Questions.Count == 0)
                    return response.SetBadRequest(message: "Quiz cần ít nhất 1 câu hỏi");

                foreach (var q in request.Questions)
                {
                    if (q.Options == null || q.Options.Count < 2)
                        return response.SetBadRequest(message: $"Câu hỏi '{q.Content}' cần ít nhất 2 đáp án");
                    var correctCount = q.Options.Count(o => o.IsCorrect);
                    if (correctCount != 1)
                        return response.SetBadRequest(message: $"Câu hỏi '{q.Content}' phải có đúng 1 đáp án đúng (hiện có {correctCount})");
                }

                await _unitOfWork.BeginTransactionAsync();

                // Create LessonItem
                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 2, // Quiz
                    OrderIndex = request.OrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                // Create GradedItem
                var gradedItem = new GradedItem
                {
                    GradedItemId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    MaxScore = (int)request.Questions.Sum(q => q.Points),
                    IsAutoGraded = true,
                    GradedItemType = 0, // Quiz type
                    SubmissionGuidelines = request.Title,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GradedItems.AddAsync(gradedItem);

                // Create Questions + AnswerOptions
                for (int qi = 0; qi < request.Questions.Count; qi++)
                {
                    var qRequest = request.Questions[qi];
                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        GradedItemId = gradedItem.GradedItemId,
                        Content = qRequest.Content,
                        Type = 0, // Multiple choice
                        Points = qRequest.Points,
                        OrderIndex = qRequest.OrderIndex > 0 ? qRequest.OrderIndex : qi,
                        IsRequired = true,
                        Explanation = qRequest.Explanation,
                        CreatedBy = claim.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Questions.AddAsync(question);

                    for (int oi = 0; oi < qRequest.Options.Count; oi++)
                    {
                        var oRequest = qRequest.Options[oi];
                        var option = new AnswerOption
                        {
                            AnswerOptionId = Guid.NewGuid(),
                            QuestionId = question.QuestionId,
                            Text = oRequest.Text,
                            IsCorrect = oRequest.IsCorrect,
                            OrderIndex = oRequest.OrderIndex > 0 ? oRequest.OrderIndex : oi,
                            Weight = oRequest.IsCorrect ? 1 : 0,
                            CreatedBy = claim.UserId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.AnswerOptions.AddAsync(option);
                    }
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Created Quiz item {ItemId} with {QuestionCount} questions for lesson {LessonId}",
                    lessonItem.LessonItemId, request.Questions.Count, request.LessonId);
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating quiz item for lesson {LessonId}", request.LessonId);
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteLessonItemAsync(Guid lessonItemId)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();
                var item = await _unitOfWork.LessonItems.GetAsync(li => li.LessonItemId == lessonItemId && !li.IsDeleted);
                if (item == null) return response.SetNotFound(message: "Không tìm thấy tài liệu");

                var courseCheck = await VerifyCourseIsDraftForLesson(item.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                // Soft delete
                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;
                item.UpdatedBy = claim.UserId;
                _unitOfWork.LessonItems.Update(item);

                // Also soft delete related resources
                var resources = await _unitOfWork.LessonResources.GetAllAsync(lr => lr.LessonItemId == lessonItemId && !lr.IsDeleted);
                foreach (var r in resources)
                {
                    r.IsDeleted = true;
                    r.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.LessonResources.Update(r);
                }

                // Soft delete graded items if quiz
                if (item.Type == 2)
                {
                    var gi = await _unitOfWork.GradedItems.GetAsync(g => g.LessonItemId == lessonItemId && !g.IsDeleted);
                    if (gi != null)
                    {
                        gi.IsDeleted = true;
                        _unitOfWork.GradedItems.Update(gi);
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                _logger.LogInformation("Deleted lesson item {ItemId}", lessonItemId);
                return response.SetOk("Đã xóa tài liệu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson item {LessonItemId}", lessonItemId);
                return response.SetBadRequest(message: ex.Message);
            }
        }

        /// <summary>
        /// Verify that the lesson's course is in Draft status and owned by the user.
        /// Returns null if OK, or an error ApiResponse.
        /// </summary>
        private async Task<ApiResponse?> VerifyCourseIsDraftForLesson(Guid lessonId, Guid userId)
        {
            var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId && !l.IsDeleted);
            if (lesson == null) return new ApiResponse().SetNotFound(message: "Không tìm thấy bài học");

            var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == lesson.ModuleId && !m.IsDeleted);
            if (module == null) return new ApiResponse().SetNotFound(message: "Không tìm thấy module");

            var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == module.CourseId && !c.IsDeleted);
            if (course == null) return new ApiResponse().SetNotFound(message: "Không tìm thấy khóa học");

            if (course.CreatedBy != userId)
                return new ApiResponse().SetBadRequest(message: "Bạn không có quyền thao tác trên khóa học này");

            if (course.Status != 0)
                return new ApiResponse().SetBadRequest(message: "Chỉ có thể chỉnh sửa khi khóa học ở trạng thái Draft");

            return null;
        }

        private static bool IsValidYouTubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            var patterns = new[]
            {
                "youtube.com/watch",
                "youtu.be/",
                "youtube.com/embed/"
            };
            return patterns.Any(p => url.Contains(p, StringComparison.OrdinalIgnoreCase));
        }
    }
}
