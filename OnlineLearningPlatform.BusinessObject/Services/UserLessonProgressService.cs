using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.UserLessonProgress;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class UserLessonProgressService : IUserLessonProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;

        public UserLessonProgressService(
            IUnitOfWork unitOfWork,
            IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
        }

        public async Task<ApiResponse> StartOrUpdateProgressAsync(UpdateUserLessonProgressRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var userId = _claimService.GetUserClaim().UserId;

                var lesson = await _unitOfWork.Lessons
                    .GetAsync(l => l.LessonId == request.LessonId);

                if (lesson == null)
                    return response.SetNotFound("Lesson not found");

                var progress = await _unitOfWork.UserLessonProgresses.GetAsync(
                    p => p.UserId == userId && p.LessonId == request.LessonId);

                // Chưa có → tạo mới
                if (progress == null)
                {
                    progress = new UserLessonProgress
                    {
                        LessonProgressId = Guid.NewGuid(),
                        UserId = userId,
                        LessonId = request.LessonId,
                        LastWatchedSecond = request.LastWatchedSecond,
                        CompletionPercent = request.CompletionPercent,
                        LastAccessedAt = DateTime.UtcNow,
                        IsCompleted = false
                    };

                    await _unitOfWork.UserLessonProgresses.AddAsync(progress);
                }
                else
                {
                    progress.LastWatchedSecond = request.LastWatchedSecond ?? progress.LastWatchedSecond;
                    progress.CompletionPercent = request.CompletionPercent ?? progress.CompletionPercent;
                    progress.LastAccessedAt = DateTime.UtcNow;

                    _unitOfWork.UserLessonProgresses.Update(progress);
                }

                await _unitOfWork.SaveChangeAsync();
                return response.SetOk(progress);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> MarkLessonCompletedAsync(Guid lessonId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var userId = _claimService.GetUserClaim().UserId;

                var progress = await _unitOfWork.UserLessonProgresses.GetAsync(
                    p => p.UserId == userId && p.LessonId == lessonId);

                if (progress == null)
                {
                    progress = new UserLessonProgress
                    {
                        LessonProgressId = Guid.NewGuid(),
                        UserId = userId,
                        LessonId = lessonId,
                        IsCompleted = true,
                        CompletedAt = DateTime.UtcNow,
                        CompletionPercent = 100,
                        LastWatchedSecond = 0,
                        LastAccessedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.UserLessonProgresses.AddAsync(progress);
                }
                else
                {
                    progress.IsCompleted = true;
                    progress.CompletedAt = DateTime.UtcNow;
                    progress.CompletionPercent = 100;
                    _unitOfWork.UserLessonProgresses.Update(progress);
                }

                //progress.IsCompleted = true;
                //progress.CompletedAt = DateTime.UtcNow;
                //progress.CompletionPercent = 100;

                //_unitOfWork.UserLessonProgresses.Update(progress);
                var lesson = await _unitOfWork.Lessons.GetAsync(l => l.LessonId == lessonId);
                var module = await _unitOfWork.Modules.GetAsync(m => m.ModuleId == lesson.ModuleId);
                var courseId = module.CourseId;

                var totalLessonInCourse = await _unitOfWork.Lessons
                    .CountAsync(l => l.Module.CourseId == courseId);

                var completedLessonsInCourse = await _unitOfWork.UserLessonProgresses.CountAsync(
                    lp => lp.UserId == userId
                       && lp.IsCompleted
                       && lp.Lesson.Module.CourseId == courseId
                );

                var enrollment = await _unitOfWork.Enrollments
                    .GetAsync(e => e.UserId == userId && e.CourseId == courseId);

                if (enrollment != null && totalLessonInCourse > 0)
                {
                    decimal percent = ((decimal)completedLessonsInCourse / totalLessonInCourse) * 100m;
                    enrollment.ProgressPercent = Math.Round(percent, 2);

                    if (enrollment.ProgressPercent >= 100)
                    {
                        enrollment.ProgressPercent = 100;
                        enrollment.Status = 2; 
                        enrollment.CompletedAt = DateTime.UtcNow;

                        var existingCert = await _unitOfWork.Certificates.GetAsync(
                            c => c.UserId == userId && c.CourseId == courseId);

                        if (existingCert == null)
                        {
                            string uniqueCode = "OLP-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

                            var newCert = new Certificate
                            {
                                CertificateId = Guid.NewGuid(),
                                UserId = userId,
                                CourseId = courseId,
                                IssueDate = DateTime.UtcNow,
                                CertificateCode = uniqueCode,
                                IsDeleted = false
                            };
                            await _unitOfWork.Certificates.AddAsync(newCert);
                        }
                    }

                    _unitOfWork.Enrollments.Update(enrollment);
                }

                await _unitOfWork.SaveChangeAsync();

                return response.SetOk("Lesson marked as completed");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }


        public async Task<ApiResponse> GetLessonProgressAsync(Guid lessonId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var userId = _claimService.GetUserClaim().UserId;

                var progress = await _unitOfWork.UserLessonProgresses.GetAsync(
                    p => p.UserId == userId && p.LessonId == lessonId);

                return response.SetOk(progress);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetLessonProgressByUserAsync(Guid userId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var progresses = await _unitOfWork.UserLessonProgresses
                    .GetAllAsync(p => p.UserId == userId);

                return response.SetOk(progresses);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}
