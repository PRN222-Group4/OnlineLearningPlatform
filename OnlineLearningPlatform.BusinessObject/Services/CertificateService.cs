using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;
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