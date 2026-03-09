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
        private readonly IStorageService _storageService;
        private readonly ILogger<LessonItemService> _logger;

        public LessonItemService(
            IUnitOfWork unitOfWork,
            IClaimService claimService,
            IStorageService storageService,
            ILogger<LessonItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _storageService = storageService;
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
                var questions = (await _unitOfWork.Questions.GetAllAsync(q => gradedItemIds.Contains(q.GradedItemId) && !q.IsDeleted)).OrderBy(q => q.OrderIndex).ToList();
                var questionIds = questions.Select(q => q.QuestionId).ToList();
                var answerOptions = (await _unitOfWork.AnswerOptions.GetAllAsync(ao => questionIds.Contains(ao.QuestionId) && !ao.IsDeleted)).OrderBy(ao => ao.OrderIndex).ToList();

                return response.SetOk(new { LessonItems = itemsList, LessonResources = resources, GradedItems = gradedItems, Questions = questions, AnswerOptions = answerOptions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson items");
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetLessonItemDetailAsync(Guid lessonItemId)
        {
            var response = new ApiResponse();
            try
            {
                var item = await _unitOfWork.LessonItems.GetAsync(li => li.LessonItemId == lessonItemId && !li.IsDeleted);
                if (item == null) return response.SetNotFound("Không tìm thấy tài liệu");
                return response.SetOk(new { LessonItem = item });
            }
            catch (Exception ex) { return response.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResponse> CreateVideoItemAsync(CreateVideoItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();
                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                string? videoUrl = null;
                if (request.VideoSourceType == 1)
                {
                    if (string.IsNullOrWhiteSpace(request.VideoUrl) || !IsValidYouTubeUrl(request.VideoUrl))
                        return response.SetBadRequest("URL YouTube không hợp lệ");
                    videoUrl = request.VideoUrl;
                }
                else if (request.VideoSourceType == 2)
                {
                    if (request.VideoFile == null) return response.SetBadRequest("File video MP4 là bắt buộc");
                    var uploadResult = await _storageService.UploadLessonResourceAsync(request.LessonId, request.Title, request.VideoFile);
                    videoUrl = uploadResult.Url;
                }

                await _unitOfWork.BeginTransactionAsync();
                int newOrderIndex = await GetNextOrderIndexAsync(request.LessonId);

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 0, // Video
                    OrderIndex = newOrderIndex,
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
                    OrderIndex = 1,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonResources.AddAsync(resource);

                await _unitOfWork.CommitAsync();
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> CreateReadingItemAsync(CreateReadingItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();
                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                await _unitOfWork.BeginTransactionAsync();
                int newOrderIndex = await GetNextOrderIndexAsync(request.LessonId);

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 1, // Reading
                    OrderIndex = newOrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                var resource = new LessonResource
                {
                    LessonResourceId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    Title = request.Title,
                    ResourceType = 0, // Text
                    TextContent = request.Content,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonResources.AddAsync(resource);
                await _unitOfWork.CommitAsync();

                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex) { await _unitOfWork.RollbackAsync(); return response.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResponse> CreateWritingItemAsync(CreateWritingItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();
                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                await _unitOfWork.BeginTransactionAsync();
                int newOrderIndex = await GetNextOrderIndexAsync(request.LessonId);

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 3, // Writing
                    OrderIndex = newOrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                var gradedItem = new GradedItem
                {
                    GradedItemId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    MaxScore = 9,
                    IsAutoGraded = true,
                    GradedItemType = 1, // 1: Tự luận / Viết
                    SubmissionGuidelines = "Write your essay based on the prompt. AI will evaluate it.",
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GradedItems.AddAsync(gradedItem);

                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    GradedItemId = gradedItem.GradedItemId,
                    Content = request.Prompt,
                    Type = 2, // 2: Essay Question
                    Points = 9,
                    OrderIndex = 1,
                    IsRequired = true,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Questions.AddAsync(question);

                await _unitOfWork.CommitAsync();
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex) { await _unitOfWork.RollbackAsync(); return response.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResponse> CreateSpeakingItemAsync(CreateSpeakingItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();
                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                await _unitOfWork.BeginTransactionAsync();
                int newOrderIndex = await GetNextOrderIndexAsync(request.LessonId);

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 4, // Speaking
                    OrderIndex = newOrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                var gradedItem = new GradedItem
                {
                    GradedItemId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    MaxScore = 9,
                    IsAutoGraded = true,
                    GradedItemType = 2, // 2: Speaking/Voice
                    SubmissionGuidelines = "Record your speaking answer. AI will evaluate fluency and pronunciation.",
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GradedItems.AddAsync(gradedItem);

                var question = new Question
                {
                    QuestionId = Guid.NewGuid(),
                    GradedItemId = gradedItem.GradedItemId,
                    Content = request.Prompt,
                    Type = 3,
                    Points = 9,
                    OrderIndex = 1,
                    IsRequired = true,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Questions.AddAsync(question);

                await _unitOfWork.CommitAsync();
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex) { await _unitOfWork.RollbackAsync(); return response.SetBadRequest(ex.Message); }
        }

        public async Task<ApiResponse> CreateQuizItemAsync(CreateQuizItemRequest request)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _claimService.GetUserClaim();
                var courseCheck = await VerifyCourseIsDraftForLesson(request.LessonId, claim.UserId);
                if (courseCheck != null) return courseCheck;

                if (request.Questions == null || request.Questions.Count == 0) return response.SetBadRequest("Quiz cần ít nhất 1 câu hỏi");

                await _unitOfWork.BeginTransactionAsync();
                int newOrderIndex = await GetNextOrderIndexAsync(request.LessonId);

                var lessonItem = new LessonItem
                {
                    LessonItemId = Guid.NewGuid(),
                    LessonId = request.LessonId,
                    Type = 2, // Quiz
                    OrderIndex = newOrderIndex,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonItems.AddAsync(lessonItem);

                var gradedItem = new GradedItem
                {
                    GradedItemId = Guid.NewGuid(),
                    LessonItemId = lessonItem.LessonItemId,
                    MaxScore = (int)request.Questions.Sum(q => q.Points),
                    IsAutoGraded = true,
                    GradedItemType = 0, // Quiz
                    SubmissionGuidelines = request.Title,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.GradedItems.AddAsync(gradedItem);

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
                        OrderIndex = qRequest.OrderIndex > 0 ? qRequest.OrderIndex : qi + 1,
                        IsRequired = true,
                        Explanation = qRequest.Explanation,
                        CreatedBy = claim.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Questions.AddAsync(question);

                    if (qRequest.Options != null)
                    {
                        for (int oi = 0; oi < qRequest.Options.Count; oi++)
                        {
                            var oRequest = qRequest.Options[oi];
                            var option = new AnswerOption
                            {
                                AnswerOptionId = Guid.NewGuid(),
                                QuestionId = question.QuestionId,
                                Text = oRequest.Text,
                                IsCorrect = oRequest.IsCorrect,
                                OrderIndex = oRequest.OrderIndex > 0 ? oRequest.OrderIndex : oi + 1,
                                Weight = oRequest.IsCorrect ? 1 : 0,
                                CreatedBy = claim.UserId,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.AnswerOptions.AddAsync(option);
                        }
                    }
                }

                await _unitOfWork.CommitAsync();
                return response.SetOk(lessonItem.LessonItemId);
            }
            catch (Exception ex) { await _unitOfWork.RollbackAsync(); return response.SetBadRequest(ex.Message); }
        }
        private async Task<int> GetNextOrderIndexAsync(Guid lessonId)
        {
            var existingItems = await _unitOfWork.LessonItems.GetAllAsync(li => li.LessonId == lessonId && !li.IsDeleted);
            return existingItems.Any() ? existingItems.Max(i => i.OrderIndex) + 1 : 1;
        }

        private async Task<ApiResponse?> VerifyCourseIsDraftForLesson(Guid lessonId, Guid userId)
        {
            var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId && !l.IsDeleted);
            if (lesson == null) return new ApiResponse().SetNotFound("Không tìm thấy bài học");
            var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == lesson.ModuleId && !m.IsDeleted);
            if (module == null) return new ApiResponse().SetNotFound("Không tìm thấy module");
            var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == module.CourseId && !c.IsDeleted);
            if (course == null) return new ApiResponse().SetNotFound("Không tìm thấy khóa học");

            if (course.CreatedBy != userId) return new ApiResponse().SetBadRequest("Bạn không có quyền thao tác trên khóa học này");
            if (course.Status != 0) return new ApiResponse().SetBadRequest("Chỉ có thể chỉnh sửa khi khóa học ở trạng thái Draft");
            return null;
        }

        public Task<ApiResponse> DeleteLessonItemAsync(Guid lessonItemId) { throw new NotImplementedException(); }
        private static bool IsValidYouTubeUrl(string url) { return true; }
    }
}