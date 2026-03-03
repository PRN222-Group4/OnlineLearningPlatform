using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.Question;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _service;

        public QuestionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
        }

        public async Task<ApiResponse> CreateQuestionAsync(CreateQuestionRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                // 1. Kiểm tra Quiz có tồn tại không
                var gradedItem = await _unitOfWork.GradedItems.GetAsync(g => g.GradedItemId == request.GradedItemId);
                if (gradedItem == null) return response.SetNotFound("Graded Item (Quiz) not found!");

                // 2. Tính số thứ tự (Index)
                var existingQuestions = await _unitOfWork.Questions.GetAllAsync(q => q.GradedItemId == request.GradedItemId && !q.IsDeleted);
                int newIndex = existingQuestions.Any() ? existingQuestions.Max(q => q.OrderIndex) + 1 : 1;

                // 3. Map dữ liệu cơ bản
                var question = _mapper.Map<Question>(request);
                question.QuestionId = Guid.NewGuid();
                question.OrderIndex = newIndex;
                question.CreatedBy = _service.GetUserClaim().UserId;

                // 4. Xử lý Đáp án (Thủ công để tránh lỗi Duplicate)
                question.AnswerOptions = new List<AnswerOption>();

                if (request.AnswerOptions != null)
                {
                    int idx = 1;
                    foreach (var optReq in request.AnswerOptions)
                    {
                        // Map từng đáp án một
                        var option = _mapper.Map<AnswerOption>(optReq);
                        option.QuestionId = question.QuestionId;
                        option.CreatedBy = question.CreatedBy;
                        option.OrderIndex = idx++;

                        question.AnswerOptions.Add(option);
                    }
                }

                // 5. Lưu xuống DB
                await _unitOfWork.Questions.AddAsync(question);
                await _unitOfWork.SaveChangeAsync();

                return response.SetOk(question);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}