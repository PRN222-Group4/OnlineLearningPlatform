using AutoMapper;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.GradedItem;
using OnlineLearningPlatform.BusinessObject.Requests.LessonItem;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class LessonItemService : ILessonItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _service;
        private readonly ILessonResourceService _resourceService;

        public LessonItemService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service, ILessonResourceService resourceService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
            _resourceService = resourceService;
        }

        public async Task<ApiResponse> CreateNewLessonItemAsync(CreateNewLessonItemRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == request.LessonId);
                if (lesson == null) return response.SetNotFound("Lesson not found");

                var lessonItem = _mapper.Map<LessonItem>(request);
                await _unitOfWork.LessonItems.AddAsync(lessonItem);
                await _unitOfWork.SaveChangeAsync();

                var gradedItemRequest = request.GradedItem;
                var questionRequests = request.GradedItem.Questions;
                if (gradedItemRequest != null)
                {
                    var gradedItem = new GradedItem()
                    {
                        MaxScore = gradedItemRequest.MaxScore,
                        SubmissionGuidelines = gradedItemRequest.SubmissionGuidelines,
                        LessonItemId = lessonItem.LessonItemId,
                    };
                    ResolveGradedItemType(gradedItem, gradedItemRequest);
                    await _unitOfWork.GradedItems.AddAsync(gradedItem);
                    await _unitOfWork.SaveChangeAsync();

                    if (gradedItem.GradedItemType == 3)
                    {
                        foreach (var q in questionRequests)
                        {
                            var question = new Question
                            {
                                GradedItemId = gradedItem.GradedItemId,
                                Content = q.Content,
                                Type = q.Type,
                            };
                            await _unitOfWork.Questions.AddAsync(question);
                            await _unitOfWork.SaveChangeAsync();

                            if (q.AnswerOptions != null)
                            {
                                foreach (var ao in q.AnswerOptions)
                                {
                                    var answerOption = new AnswerOption
                                    {
                                        QuestionId = question.QuestionId,
                                        Text = ao.Text,
                                        IsCorrect = ao.IsCorrect,
                                        Explanation = ao.Explanation,
                                        OrderIndex = ao.OrderIndex,
                                        Weight = ao.Weight,
                                    };
                                    await _unitOfWork.AnswerOptions.AddAsync(answerOption);
                                    await _unitOfWork.SaveChangeAsync();
                                }
                            }
                        }
                        lessonItem.GradedItem = gradedItem;
                    }
                }
                if (request.LessonResources != null)
                {
                    foreach (var lessonResource in request.LessonResources)
                    {
                        lessonResource.LessonItemId = lessonItem.LessonItemId;
                        var lessonResourceResponse = await _resourceService.CreateLessonResourceAsync(lessonResource);
                    }
                }
                lessonItem.Type = ResolveLessonItemType(lessonItem);
                await _unitOfWork.SaveChangeAsync();
                return response.SetOk("Create Lesson Item, Graded Item, Question And Answer Option successfully ^^");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }
        private int ResolveLessonItemType(LessonItem item)
        {
            // 1. Ưu tiên GradedItem
            if (item.GradedItem != null)
            {
                return item.GradedItem.GradedItemType switch
                {
                    3 => 5,
                    1 => 6,
                    0 => 7,
                    2 => 4,
                    _ => throw new Exception("Invalid graded item type")
                };
            }

            // 2. Fallback: dựa vào LessonResources
            if (item.LessonResources == null || !item.LessonResources.Any())
            {
                throw new Exception(
                    "LessonItem must have either GradedItem or at least one LessonResource"
                );
            }

            // Resource chính = OrderIndex nhỏ nhất
            var mainResource = item.LessonResources
                .OrderBy(r => r.OrderIndex)
                .First();

            return mainResource.ResourceType switch
            {
                0 => 0,
                6 => 3,
                3 => 1,
                1 => 1,
                2 => 1,
                5 => 1,
                4 => 1,
                _ => throw new Exception("Unsupported resource type")
            };
        }
        private void ResolveGradedItemType(GradedItem item, CreateGradedItemRequest request)
        {
            var hasQuestions = request.Questions != null && request.Questions.Any();
            var hasSubmissionGuidelines = !string.IsNullOrWhiteSpace(item.SubmissionGuidelines);

            if (hasQuestions)
            {
                item.GradedItemType = 3;
                item.IsAutoGraded = true;
                return;
            }

            if (hasSubmissionGuidelines)
            {
                // ⚠️ Không phân biệt được Writing / Assignment / Speaking
                item.GradedItemType = 0;
                item.IsAutoGraded = false;
                return;
            }

            throw new Exception("Invalid graded item: must have Questions or SubmissionGuidelines");
        }
    }
}
