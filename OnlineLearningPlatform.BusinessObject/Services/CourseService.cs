using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class CourseService : ICourseService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly IClaimService _service;
        private readonly IEmailService _emailService;

        public CourseService(IMapper mapper, IUnitOfWork unitOfWork, IFirebaseStorageService firebaseStorageService, IClaimService service, IEmailService emailService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _firebaseStorageService = firebaseStorageService;
            _service = service;
            _emailService = emailService;
        }

        public async Task<ApiResponse> CreateNewCourseAsync(CreateNewCourseRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var course = _mapper.Map<Course>(request);
                course.CourseId = Guid.NewGuid();
                course.CreatedBy = claim.UserId;
                if (request.ImageFile != null)
                {
                    var imageUrl = await _firebaseStorageService.UploadCourseImage(request.Title, request.ImageFile);
                    course.Image = imageUrl;
                }
                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.SaveChangeAsync();

                return response.SetOk(course.CourseId);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetAllCourseAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var userId = _service.GetUserClaim().UserId;
                var courses = await _unitOfWork.Courses.GetAllAsync(c => c.Status == 2 && c.CreatedBy != userId && !c.Enrollments.Any(e => e.UserId == userId));
                var courseResponses = _mapper.Map<List<CourseResponse>>(courses);
                return response.SetOk(courseResponses);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetCourseDetailAsync(Guid courseId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId);
                if (course == null)
                {
                    return response.SetNotFound("Course not found");
                }
                var courseResponse = _mapper.Map<CourseResponse>(course);
                return response.SetOk(courseResponse);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateCourseAsync(UpdateCourseRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == request.CourseId);
                if (course == null)
                {
                    return response.SetNotFound("Course not found");
                }
                var userId = _service.GetUserClaim().UserId;
                var updatedCourse = _mapper.Map(request, course);
                updatedCourse.UpdatedAt = DateTime.UtcNow;
                updatedCourse.UpdatedBy = userId;

                _unitOfWork.Courses.Update(updatedCourse);
                await _unitOfWork.SaveChangeAsync();
                return response.SetOk("Course updated successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteCourseAsync(Guid courseId)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId);
                if (course == null)
                {
                    return response.SetNotFound("Course not found");
                }
                course.IsDeleted = true;
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangeAsync();
                return response.SetOk("Course deleted successfully");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetAllCourseForAdminAsync(int status)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                // status == -1 means return all courses regardless of status
                var courses = status == -1
                    ? await _unitOfWork.Courses.GetAllAsync(c => !c.IsDeleted)
                    : await _unitOfWork.Courses.GetAllAsync(c => !c.IsDeleted && c.Status == status);
                if (courses == null) return null;

                var result = new List<GetAllCourseForAdminResponse>();

                foreach (var course in courses)
                {
                    var modules = await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == course.CourseId);
                    var moduleIds = modules.Select(m => m.ModuleId).ToList();

                    var lessons = await _unitOfWork.Lessons.GetAllAsync(l => moduleIds.Contains(l.ModuleId));
                    var lessonIds = lessons.Select(l => l.LessonId).ToList();
                    var lessonItems = await _unitOfWork.LessonItems.GetAllAsync(l => lessonIds.Contains(l.LessonId));
                    var lessonItemIds = lessonItems.Select(li => li.LessonItemId).ToList();
                    var courseMapping = _mapper.Map<GetAllCourseForAdminResponse>(course);

                    courseMapping.ModuleCount = modules.Count;
                    courseMapping.LessonCount = lessons.Count;
                    courseMapping.VideoCount = lessonItems.Count(li => li.Type == 0);
                    courseMapping.ReadingCount = lessonItems.Count(l => l.Type == 1);

                    result.Add(courseMapping);
                }
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetCoursesByInstructorAsync()
        {
            ApiResponse response = new ApiResponse();

            try
            {
                var claim = _service.GetUserClaim();
                var courses = await _unitOfWork.Courses
                    .GetAllAsync(c => c.CreatedBy == claim.UserId && !c.IsDeleted);

                var result = _mapper.Map<List<CourseResponse>>(courses);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetEnrolledCoursesForStudentAsync()
        {
            ApiResponse response = new ApiResponse();

            try
            {
                var studentId = _service.GetUserClaim().UserId;
                var enrollments = await _unitOfWork.Enrollments
                    .GetAllAsync(e => e.UserId == studentId);

                var courseIds = enrollments
                    .Select(e => e.CourseId)
                    .Distinct()
                    .ToList();

                var courses = await _unitOfWork.Courses
                    .GetAllAsync(c => courseIds.Contains(c.CourseId));

                var result = _mapper.Map<List<CourseResponse>>(courses);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> ApproveCourseAsync(ApproveCourseRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == request.CourseId && !c.IsDeleted);
                if (course == null) return response.SetNotFound("Course not found");

                var instructor = await _unitOfWork.Users.GetAsync(u => u.UserId == course.CreatedBy);

                if (!request.Status)
                {
                    if (string.IsNullOrEmpty(request.RejectReason))
                        return response.SetBadRequest("Reject reason is required");

                    course.Status = 3;
                    course.RejectReason = request.RejectReason;
                    course.UpdatedAt = DateTime.UtcNow;
                    course.UpdatedBy = _service.GetUserClaim().UserId;

                    _unitOfWork.Courses.Update(course);
                    await _unitOfWork.SaveChangeAsync();

                    if (instructor != null)
                    {
                        await _emailService.SendRejectCourseEmail(instructor.FullName, instructor.Email, request.RejectReason, course.Title);
                    }

                    return response.SetOk("Course rejected & email sent");
                }
                else
                {
                    course.Status = 2;
                    course.RejectReason = string.Empty;
                    course.UpdatedAt = DateTime.UtcNow;
                    course.UpdatedBy = _service.GetUserClaim().UserId;

                    _unitOfWork.Courses.Update(course);
                    await _unitOfWork.SaveChangeAsync();


                    if (instructor != null)
                    {
                        await _emailService.SendApproveCourseEmail(
                            receiverName: instructor.FullName,
                            receiverEmail: instructor.Email,
                            courseTitle: course.Title
                        );
                    }

                    return response.SetOk("Course approved & email sent");
                }
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> SubmitCourseForReviewAsync(Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId);
                if (course == null) return response.SetNotFound("Course not found");
                course.Status = 1;

                await _unitOfWork.SaveChangeAsync();
                return response.SetOk("Course submitted for review.");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetCoursesByStatusAsync(int status)
        {
            var response = new ApiResponse();
            try
            {
                var courses = await _unitOfWork.Courses.GetAllAsync(c => c.Status == status && !c.IsDeleted);

                var courseResponses = _mapper.Map<List<CourseResponse>>(courses);
                //Console.WriteLine(courseResponses);

                return response.SetOk(courseResponses);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetCourseByIdAsync(Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId);
                if (course == null) return response.SetNotFound("Course not found");

                var courseResponse = _mapper.Map<CourseResponse>(course);
                return response.SetOk(courseResponse);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetCourseDetailForStudentAsync(Guid courseId)
        {
            try
            {
                var userId = _service.GetUserClaim().UserId;
                // 1. Lấy thông tin Course
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId);
                if (course == null) return new ApiResponse().SetNotFound("Course not found");

                var instructor = await _unitOfWork.Users.GetAsync(u => u.UserId == course.CreatedBy);

                // 2. Lấy Modules & Lessons (Query thủ công để gom hết vào 1 cục)
                var modules = await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == courseId && !m.IsDeleted);
                var moduleIds = modules.Select(m => m.ModuleId).ToList();
                var lessons = await _unitOfWork.Lessons.GetAllAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted);
                var lessonIds = lessons.Select(l => l.LessonId).ToList();
                var lessonItems = await _unitOfWork.LessonItems.GetAllAsync(li => lessonIds.Contains(li.LessonId) && !li.IsDeleted);
                var lessonItemIds = lessonItems.Select(li => li.LessonItemId).ToList();
                var lessonResources = await _unitOfWork.LessonResources.GetAllAsync(lr => lessonItemIds.Contains(lr.LessonItemId) && !lr.IsDeleted);
                var gradedItems = await _unitOfWork.GradedItems.GetAllAsync(gi => lessonItemIds.Contains(gi.LessonItemId));
                var attempt = await _unitOfWork.GradedAttempts.GetAllAsync(ga => ga.UserId == userId && gradedItems.Select(gi => gi.GradedItemId).Contains(ga.GradedItemId));

                var response = _mapper.Map<StudentCourseDetailResponse>(course);
                return new ApiResponse().SetOk(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse().SetBadRequest(ex.Message);
            }
        }

    }
}