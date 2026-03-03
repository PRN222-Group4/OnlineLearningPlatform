using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.LessonResource;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.LessonResource;
using Microsoft.AspNetCore.Http;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class LessonResourceService : ILessonResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _service;
        private readonly IFirebaseStorageService _storage;

        public LessonResourceService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service, IFirebaseStorageService storage)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
            _storage = storage;
        }

        public async Task<ApiResponse> CreateLessonResourceAsync(CreateLessonResourceRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();

                // 1. Kiểm tra Lesson tồn tại
                var lessonItem = await _unitOfWork.LessonItems.GetAsync(li => li.LessonItemId == request.LessonItemId);
                if (lessonItem == null)
                {
                    return response.SetNotFound("LessonItem not found or may have been automatically deleted due to inactivity!!! Please check your course");
                }

                if (request.File == null && string.IsNullOrWhiteSpace(request.TextContent))
                {
                    return response.SetBadRequest(message: "Add File Or Text Content =,=");
                }
                if (request.File != null && !string.IsNullOrWhiteSpace(request.TextContent))
                {
                    return response.SetBadRequest(
                        message: "Only one resource type is allowed: File OR TextContent"
                    );
                }

                var lessonResource = _mapper.Map<LessonResource>(request);

                if (request.File != null)
                {
                    // 4. Upload lên Firebase
                    var uploadResource = await _storage.UploadLessonResourceAsync(request.LessonItemId, request.Title, request.File);

                    // 5. Gán các thông tin
                    lessonResource.ResourceUrl = uploadResource.Url;
                    lessonResource.ResourceType = uploadResource.Type;
                    lessonResource.CreatedBy = claim.UserId;

                }
                else
                {
                    lessonResource.TextContent = request.TextContent;
                    lessonResource.ResourceType = 3;
                    lessonResource.CreatedBy = claim.UserId;
                }
                await _unitOfWork.LessonResources.AddAsync(lessonResource);
                await _unitOfWork.SaveChangeAsync();

                var result = _mapper.Map<LessonResourceResponse>(lessonResource);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetResourcesByLessonItemAsync(Guid lessonItemId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var lessonItem = await _unitOfWork.LessonItems.GetAsync(l => l.LessonItemId == lessonItemId);
                if (lessonItem == null)
                    return response.SetNotFound("Lesson Item not found");

                // Lấy resource chưa bị xóa
                var resources = await _unitOfWork.LessonResources.GetAllAsync(
                    r => r.LessonItemId == lessonItemId && !r.IsDeleted
                );

                // ===> SẮP XẾP TRƯỚC KHI TRẢ VỀ <===
                // Frontend sẽ hiển thị đúng thứ tự nhờ dòng này
                var sortedResources = resources.OrderBy(r => r.OrderIndex).ToList();

                var result = _mapper.Map<List<LessonResourceResponse>>(sortedResources);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateLessonResourceAsync(Guid resourceId, UpdateLessonResourceRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var resource = await _unitOfWork.LessonResources
                    .GetAsync(r => r.LessonResourceId == resourceId);

                if (resource == null)
                    return response.SetNotFound("Lesson resource not found");

                // Update basic info
                resource.Title = request.Title ?? resource.Title;
                resource.UpdatedBy = _service.GetUserClaim().UserId;

                // Update file nếu có
                if (request.File != null)
                {
                    // Logic xóa file cũ nếu cần thiết có thể bỏ comment
                    /* if (!string.IsNullOrEmpty(resource.ResourceUrl))
                    {
                        await _storage.DeleteAsync(resource.ResourceUrl);
                    }*/

                    var upload = await _storage.UploadLessonResourceAsync(
                        resource.LessonItemId,
                        resource.Title,
                        request.File
                    );

                    resource.ResourceUrl = upload.Url;
                    resource.ResourceType = upload.Type;
                }

                await _unitOfWork.SaveChangeAsync();
                return response.SetOk("Lesson resource updated successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteLessonResourceAsync(Guid resourceId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var resource = await _unitOfWork.LessonResources
                    .GetAsync(r => r.LessonResourceId == resourceId);

                if (resource == null)
                    return response.SetNotFound("Lesson resource not found");

                _unitOfWork.LessonResources.RemoveIdAsync(resource.LessonResourceId);

                await _unitOfWork.SaveChangeAsync();

                return response.SetOk("Lesson resource deleted successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}