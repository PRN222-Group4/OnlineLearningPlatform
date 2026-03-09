using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.GradedItem;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class GradedItemService : IGradedItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _service;

        public GradedItemService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
        }

        public async Task<ApiResponse> SubmitQuizAsync(SubmitQuizRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var userId = _service.GetUserClaim().UserId;

                var gradedItem = await _unitOfWork.GradedItems.GetAsync(
                    g => g.GradedItemId == request.GradedItemId,
                    include: g => g.Include(x => x.Questions) 
                );

                if (gradedItem == null)
                    return response.SetNotFound("Quiz not found");

                if (!gradedItem.IsAutoGraded)
                    return response.SetBadRequest("This quiz is not auto-graded");

                var attempt = new GradedAttempt
                {
                    GradedAttemptId = Guid.NewGuid(),
                    GradedItemId = gradedItem.GradedItemId,
                    UserId = userId,
                    SubmittedAt = DateTime.UtcNow,
                    QuestionSubmissions = new List<QuestionSubmission>(),
                    Status = 1
                };

                decimal totalScore = 0;

                foreach (var answer in request.Answers)
                {
                    var question = gradedItem.Questions
                        .First(q => q.QuestionId == answer.QuestionId);

                    if (question.Type == 2)
                        continue;

                    var correctOptions = await _unitOfWork.AnswerOptions
                        .GetAllAsync(a => a.QuestionId == question.QuestionId && a.IsCorrect);

                    var isCorrect =
                        correctOptions.Select(o => o.AnswerOptionId).OrderBy(x => x)
                        .SequenceEqual(answer.SelectedAnswerOptionIds.OrderBy(x => x));

                    var questionScore = isCorrect ? question.Points : 0;

                    totalScore += questionScore;

                    attempt.QuestionSubmissions.Add(new QuestionSubmission
                    {
                        QuestionSubmissionId = Guid.NewGuid(),
                        QuestionId = question.QuestionId,
                        Score = questionScore,
                        SubmissionAnswerOptions = answer.SelectedAnswerOptionIds
                            .Select(id => new SubmissionAnswerOption
                            {
                                AnswerOptionId = id
                            }).ToList()
                    });
                }

                attempt.Score = totalScore;
                attempt.Status = 1;
                await _unitOfWork.GradedAttempts.AddAsync(attempt);
                await _unitOfWork.SaveChangeAsync();

                return response.SetOk(new
                {
                    attempt.Score,
                    MaxScore = gradedItem.MaxScore,
                    attempt.Status
                });
            }
            catch (Exception ex)
            {
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return response.SetBadRequest($"Lỗi DB thật sự nè: {realError}");
            }
        }

    }
}
