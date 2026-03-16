using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Certificate;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CertificateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse> VerifyCertificateAsync(string certificateCode)
        {
            var response = new ApiResponse();
            try
            {
                if (string.IsNullOrWhiteSpace(certificateCode))
                    return response.SetBadRequest("Mã bằng cấp không được để trống.");

                var cert = await _unitOfWork.Certificates.GetQueryable()
                    .Include(c => c.User)
                    .Include(c => c.Course)
                    .FirstOrDefaultAsync(c => c.CertificateCode == certificateCode && !c.IsDeleted);

                if (cert == null)
                    return response.SetNotFound("Không tìm thấy bằng cấp hoặc bằng cấp đã bị thu hồi.");

                var result = new CertificateVerificationResponse
                {
                    StudentName = cert.User.FullName ?? cert.User.Email,
                    CourseName = cert.Course.Title,
                    IssueDate = cert.IssueDate,
                    CertificateCode = cert.CertificateCode,
                    CertificateUrl = cert.CertificateUrl
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetMyCertificatesAsync(Guid userId)
        {
            var response = new ApiResponse();
            try
            {
                var certificates = await _unitOfWork.Certificates.GetAllAsync(
                    c => c.UserId == userId && !c.IsDeleted,
                    include: c => c.Include(x => x.Course).Include(x => x.User)
                );

                var result = new List<CertificateResponse>();

                foreach (var cert in certificates.OrderByDescending(c => c.IssueDate))
                {
                    var instructor = await _unitOfWork.Users.GetAsync(u => u.UserId == cert.Course.CreatedBy);

                    result.Add(new CertificateResponse
                    {
                        CertificateId = cert.CertificateId,
                        CourseTitle = cert.Course.Title,
                        CourseImage = cert.Course.Image ?? "https://placehold.co/600x400/e2e8f0/475569?text=Course+Image",
                        InstructorName = instructor?.FullName ?? "CourseSphere Instructor",
                        StudentName = cert.User.FullName,
                        IssueDate = cert.IssueDate,
                        CertificateCode = cert.CertificateCode
                    });
                }

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}