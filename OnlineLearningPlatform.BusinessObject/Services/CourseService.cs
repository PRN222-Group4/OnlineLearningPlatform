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
                course.Status = 0; // Draft
                course.CreatedAt = DateTime.UtcNow;
                course.Subtitle = request.Subtitle;
                course.Tags = request.Tags;

                if (request.ImageFile != null)
                {
                    var imageUrl = await _firebaseStorageService.UploadCourseImage(request.Title, request.ImageFile);
                    course.Image = imageUrl;
                }

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.Courses.AddAsync(course);

                // Auto-create a default Module
                var defaultModule = new Module
                {
                    ModuleId = Guid.NewGuid(),
                    CourseId = course.CourseId,
                    Name = "Main",
                    Description = "Default module",
                    Index = 0,
                    IsPublished = true,
                    CreatedBy = claim.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Modules.AddAsync(defaultModule);
                await _unitOfWork.CommitAsync();

                return response.SetOk(course.CourseId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
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
                var claim = _service.GetUserClaim();
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == request.CourseId && !c.IsDeleted);
                if (course == null)
                {
                    return response.SetNotFound("Course not found");
                }
                if (course.CreatedBy != claim.UserId)
                {
                    return response.SetBadRequest(message: "Bạn không có quyền cập nhật khóa học này");
                }
                if (course.Status != 0)
                {
                    return response.SetBadRequest(message: "Chỉ có thể chỉnh sửa khóa học ở trạng thái Draft");
                }

                var updatedCourse = _mapper.Map(request, course);
                if (request.ImageFile != null)
                {
                    var imageUrl = await _firebaseStorageService.UploadCourseImage(request.Title, request.ImageFile);
                    updatedCourse.Image = imageUrl;
                }
                updatedCourse.UpdatedAt = DateTime.UtcNow;
                updatedCourse.UpdatedBy = claim.UserId;

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
                var claim = _service.GetUserClaim();
                if (claim.Role != 0)
                {
                    return response.SetBadRequest(message: "Chỉ Admin có quyền duyệt khóa học");
                }

                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == request.CourseId && !c.IsDeleted);
                if (course == null) return response.SetNotFound("Course not found");
                if (course.Status != 1) return response.SetBadRequest(message: "Chỉ có thể duyệt/từ chối khóa học ở trạng thái Pending");

                var instructor = await _unitOfWork.Users.GetAsync(u => u.UserId == course.CreatedBy);
                var adminId = claim.UserId;

                if (!request.Status)
                {
                    // Reject → Draft
                    if (string.IsNullOrEmpty(request.RejectReason))
                        return response.SetBadRequest("Reject reason is required");

                    course.Status = 0; // Back to Draft
                    course.RejectReason = request.RejectReason;
                    course.RejectedAt = DateTime.UtcNow;
                    course.UpdatedAt = DateTime.UtcNow;
                    course.UpdatedBy = adminId;

                    await _unitOfWork.BeginTransactionAsync();
                    _unitOfWork.Courses.Update(course);
                    await _unitOfWork.CommitAsync();

                    if (instructor != null)
                    {
                        await _emailService.SendRejectCourseEmail(instructor.FullName, instructor.Email, request.RejectReason, course.Title);
                    }

                    return response.SetOk("Course rejected & email sent");
                }
                else
                {
                    // Approve → Published
                    course.Status = 2;
                    course.RejectReason = null;
                    course.PublishedAt = DateTime.UtcNow;
                    course.UpdatedAt = DateTime.UtcNow;
                    course.UpdatedBy = adminId;

                    await _unitOfWork.BeginTransactionAsync();
                    _unitOfWork.Courses.Update(course);
                    await _unitOfWork.CommitAsync();

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
                await _unitOfWork.RollbackAsync();
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> SubmitCourseForReviewAsync(Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId && !c.IsDeleted);
                if (course == null) return response.SetNotFound("Course not found");
                if (course.CreatedBy != claim.UserId)
                    return response.SetBadRequest(message: "Bạn không có quyền submit khóa học này");
                if (course.Status != 0)
                    return response.SetBadRequest(message: "Chỉ có thể submit khóa học ở trạng thái Draft");

                await _unitOfWork.BeginTransactionAsync();
                course.Status = 1;
                course.SubmittedAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;
                course.UpdatedBy = claim.UserId;
                _unitOfWork.Courses.Update(course);

                await _unitOfWork.CommitAsync();
                return response.SetOk("Course submitted for review.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetCoursesByStatusAsync(int status)
        {
            var response = new ApiResponse();
            try
            {
                var courses = await _unitOfWork.Courses.GetAllAsync(c => c.Status == status && !c.IsDeleted);
                if (status == 2)
                {
                    var courseResponses = _mapper.Map<List<CourseResponse>>(courses);
                    return response.SetOk(courseResponses);
                }
                return response.SetOk(courses.ToList());
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
            var response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                if (claim.Role != 2)
                {
                    return response.SetBadRequest(message: "Only students can access the learning page.");
                }

                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId && !c.IsDeleted);
                if (course == null)
                {
                    return response.SetNotFound("Course not found");
                }

                if (course.Status != 2)
                {
                    return response.SetBadRequest(message: "This course is not published yet.");
                }

                var enrollment = await _unitOfWork.Enrollments.GetAsync(e =>
                    e.UserId == claim.UserId
                    && e.CourseId == courseId
                    && (e.Status == 1 || e.Status == 2)
                    && !e.IsDeleted);

                if (enrollment == null)
                {
                    return response.SetBadRequest(message: "You are not enrolled in this course.");
                }

                var hasSuccessfulPayment = await _unitOfWork.Payments.AnyAsync(p =>
                    p.UserId == claim.UserId
                    && p.CourseId == courseId
                    && p.Status == 1
                    && !p.IsDeleted);

                if (course.Price > 0 && !hasSuccessfulPayment)
                {
                    return response.SetBadRequest(message: "Payment is required before accessing this course.");
                }

                var modules = (await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == courseId && !m.IsDeleted))
                    .OrderBy(m => m.Index)
                    .ToList();
                var moduleIds = modules.Select(m => m.ModuleId).ToList();

                var lessons = (await _unitOfWork.Lessons.GetAllAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted))
                    .OrderBy(l => l.OrderIndex)
                    .ToList();
                var lessonIds = lessons.Select(l => l.LessonId).ToList();

                var lessonItems = (await _unitOfWork.LessonItems.GetAllAsync(li => lessonIds.Contains(li.LessonId) && !li.IsDeleted))
                    .OrderBy(li => li.OrderIndex)
                    .ToList();
                var lessonItemIds = lessonItems.Select(li => li.LessonItemId).ToList();

                var lessonResources = (await _unitOfWork.LessonResources.GetAllAsync(lr => lessonItemIds.Contains(lr.LessonItemId) && !lr.IsDeleted))
                    .OrderBy(lr => lr.OrderIndex)
                    .ToList();

                var gradedItems = await _unitOfWork.GradedItems.GetAllAsync(gi => lessonItemIds.Contains(gi.LessonItemId) && !gi.IsDeleted);
                var gradedItemIds = gradedItems.Select(gi => gi.GradedItemId).ToList();

                var questions = (await _unitOfWork.Questions.GetAllAsync(q => gradedItemIds.Contains(q.GradedItemId) && !q.IsDeleted))
                    .OrderBy(q => q.OrderIndex)
                    .ToList();
                var questionIds = questions.Select(q => q.QuestionId).ToList();

                var answerOptions = (await _unitOfWork.AnswerOptions.GetAllAsync(ao => questionIds.Contains(ao.QuestionId) && !ao.IsDeleted))
                    .OrderBy(ao => ao.OrderIndex)
                    .ToList();

                var progressRows = await _unitOfWork.UserLessonProgresses.GetAllAsync(p =>
                    p.UserId == claim.UserId
                    && lessonIds.Contains(p.LessonId));

                var progressByLesson = progressRows
                    .GroupBy(p => p.LessonId)
                    .ToDictionary(g => g.Key, g => g.Any(x => x.IsCompleted));
                var gradedByLessonItem = gradedItems.ToDictionary(g => g.LessonItemId, g => g);

                var result = new StudentLearningDetailResponse
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    ProgressPercent = enrollment.ProgressPercent,
                    Modules = modules.Select(module => new StudentLearningModuleResponse
                    {
                        ModuleId = module.ModuleId,
                        Title = module.Name,
                        OrderIndex = module.Index,
                        Lessons = lessons
                            .Where(lesson => lesson.ModuleId == module.ModuleId)
                            .OrderBy(lesson => lesson.OrderIndex)
                            .Select(lesson => new StudentLearningLessonResponse
                            {
                                LessonId = lesson.LessonId,
                                Title = lesson.Title,
                                Description = lesson.Description,
                                OrderIndex = lesson.OrderIndex,
                                EstimatedMinutes = lesson.EstimatedMinutes,
                                IsCompleted = progressByLesson.TryGetValue(lesson.LessonId, out var isCompleted) && isCompleted,
                                Materials = lessonItems
                                    .Where(item => item.LessonId == lesson.LessonId)
                                    .OrderBy(item => item.OrderIndex)
                                    .Select(item =>
                                    {
                                        var firstResource = lessonResources
                                            .Where(resource => resource.LessonItemId == item.LessonItemId)
                                            .OrderBy(resource => resource.OrderIndex)
                                            .FirstOrDefault();

                                        var material = new StudentLearningMaterialResponse
                                        {
                                            LessonItemId = item.LessonItemId,
                                            Type = item.Type,
                                            OrderIndex = item.OrderIndex,
                                            Title = firstResource?.Title ?? GetMaterialTypeLabel(item.Type),
                                            Description = lesson.Description,
                                            VideoUrl = firstResource?.ResourceUrl,
                                            Content = firstResource?.TextContent
                                        };

                                        if (item.Type == 2 && gradedByLessonItem.TryGetValue(item.LessonItemId, out var gradedItem))
                                        {
                                            var quizQuestions = questions
                                                .Where(question => question.GradedItemId == gradedItem.GradedItemId)
                                                .OrderBy(question => question.OrderIndex)
                                                .Select(question => new StudentLearningQuestionResponse
                                                {
                                                    QuestionId = question.QuestionId,
                                                    Content = question.Content,
                                                    OrderIndex = question.OrderIndex,
                                                    Options = answerOptions
                                                        .Where(option => option.QuestionId == question.QuestionId)
                                                        .OrderBy(option => option.OrderIndex)
                                                        .Select(option => new StudentLearningAnswerOptionResponse
                                                        {
                                                            AnswerOptionId = option.AnswerOptionId,
                                                            Text = option.Text,
                                                            OrderIndex = option.OrderIndex
                                                        })
                                                        .ToList()
                                                })
                                                .ToList();

                                            material.Quiz = new StudentLearningQuizResponse
                                            {
                                                GradedItemId = gradedItem.GradedItemId,
                                                Title = firstResource?.Title ?? "Quiz",
                                                Questions = quizQuestions
                                            };
                                        }

                                        return material;
                                    })
                                    .ToList()
                            })
                            .ToList()
                    }).ToList()
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetCourseForEditAsync(Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId && !c.IsDeleted);
                if (course == null) return response.SetNotFound(message: "Không tìm thấy khóa học");

                // Permission: owner (instructor) or admin (review)
                if (course.CreatedBy != claim.UserId && claim.Role != 0)
                    return response.SetBadRequest(message: "Bạn không có quyền chỉnh sửa khóa học này");

                // Load modules + lessons + lesson items + resources + graded items + questions + answer options
                var modules = (await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == courseId && !m.IsDeleted))
                    .OrderBy(m => m.Index).ToList();
                var moduleIds = modules.Select(m => m.ModuleId).ToList();

                var lessons = (await _unitOfWork.Lessons.GetAllAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted))
                    .OrderBy(l => l.OrderIndex).ToList();
                var lessonIds = lessons.Select(l => l.LessonId).ToList();

                var lessonItems = (await _unitOfWork.LessonItems.GetAllAsync(li => lessonIds.Contains(li.LessonId) && !li.IsDeleted))
                    .OrderBy(li => li.OrderIndex).ToList();
                var lessonItemIds = lessonItems.Select(li => li.LessonItemId).ToList();

                var lessonResources = (await _unitOfWork.LessonResources.GetAllAsync(lr => lessonItemIds.Contains(lr.LessonItemId) && !lr.IsDeleted)).ToList();
                var gradedItems = (await _unitOfWork.GradedItems.GetAllAsync(gi => lessonItemIds.Contains(gi.LessonItemId) && !gi.IsDeleted)).ToList();

                var gradedItemIds = gradedItems.Select(gi => gi.GradedItemId).ToList();
                var questions = (await _unitOfWork.Questions.GetAllAsync(q => gradedItemIds.Contains(q.GradedItemId) && !q.IsDeleted))
                    .OrderBy(q => q.OrderIndex).ToList();
                var questionIds = questions.Select(q => q.QuestionId).ToList();
                var answerOptions = (await _unitOfWork.AnswerOptions.GetAllAsync(ao => questionIds.Contains(ao.QuestionId) && !ao.IsDeleted))
                    .OrderBy(ao => ao.OrderIndex).ToList();

                var result = new CourseEditBundleResponse
                {
                    Course = new CourseEditSummaryResponse
                    {
                        CourseId = course.CourseId,
                        LanguageId = course.LanguageId,
                        Title = course.Title,
                        Subtitle = course.Subtitle,
                        Description = course.Description,
                        Image = course.Image,
                        Status = course.Status,
                        Price = course.Price,
                        Level = course.Level,
                        Tags = course.Tags,
                        SubmittedAt = course.SubmittedAt
                    },
                    Modules = modules.Select(module => new CourseModuleEditResponse
                    {
                        ModuleId = module.ModuleId,
                        CourseId = module.CourseId,
                        Name = module.Name,
                        Description = module.Description,
                        Index = module.Index
                    }).ToList(),
                    Lessons = lessons.Select(lesson => new CourseLessonEditResponse
                    {
                        LessonId = lesson.LessonId,
                        ModuleId = lesson.ModuleId,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        EstimatedMinutes = lesson.EstimatedMinutes,
                        OrderIndex = lesson.OrderIndex
                    }).ToList(),
                    LessonItems = lessonItems.Select(item => new CourseLessonItemEditResponse
                    {
                        LessonItemId = item.LessonItemId,
                        LessonId = item.LessonId,
                        Type = item.Type,
                        OrderIndex = item.OrderIndex
                    }).ToList(),
                    LessonResources = lessonResources.Select(resource => new CourseLessonResourceEditResponse
                    {
                        LessonResourceId = resource.LessonResourceId,
                        LessonItemId = resource.LessonItemId,
                        Title = resource.Title,
                        ResourceType = resource.ResourceType,
                        ResourceUrl = resource.ResourceUrl,
                        TextContent = resource.TextContent,
                        VideoSourceType = resource.VideoSourceType,
                        OrderIndex = resource.OrderIndex
                    }).ToList(),
                    GradedItems = gradedItems.Select(item => new CourseGradedItemEditResponse
                    {
                        GradedItemId = item.GradedItemId,
                        LessonItemId = item.LessonItemId
                    }).ToList(),
                    Questions = questions.Select(question => new CourseQuestionEditResponse
                    {
                        QuestionId = question.QuestionId,
                        GradedItemId = question.GradedItemId,
                        Content = question.Content,
                        OrderIndex = question.OrderIndex
                    }).ToList(),
                    AnswerOptions = answerOptions.Select(option => new CourseAnswerOptionEditResponse
                    {
                        AnswerOptionId = option.AnswerOptionId,
                        QuestionId = option.QuestionId,
                        Text = option.Text,
                        IsCorrect = option.IsCorrect,
                        OrderIndex = option.OrderIndex
                    }).ToList()
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> ValidateAndSubmitForReviewAsync(Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var course = await _unitOfWork.Courses.GetAsync(c => c.CourseId == courseId && !c.IsDeleted);
                if (course == null) return response.SetNotFound(message: "Không tìm thấy khóa học");
                if (course.CreatedBy != claim.UserId)
                    return response.SetBadRequest(message: "Bạn không có quyền submit khóa học này");
                if (course.Status != 0)
                    return response.SetBadRequest(message: "Chỉ có thể submit khóa học ở trạng thái Draft");

                // Validate: at least 1 lesson with at least 1 material
                var modules = await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == courseId && !m.IsDeleted);
                var moduleIds = modules.Select(m => m.ModuleId).ToList();
                var lessons = await _unitOfWork.Lessons.GetAllAsync(l => moduleIds.Contains(l.ModuleId) && !l.IsDeleted);

                if (!lessons.Any())
                    return response.SetBadRequest(message: "Khóa học cần ít nhất 1 bài học trước khi submit");

                var lessonIds = lessons.Select(l => l.LessonId).ToList();
                var lessonItems = await _unitOfWork.LessonItems.GetAllAsync(li => lessonIds.Contains(li.LessonId) && !li.IsDeleted);

                foreach (var lesson in lessons)
                {
                    var itemsForLesson = lessonItems.Where(li => li.LessonId == lesson.LessonId).ToList();
                    if (!itemsForLesson.Any())
                        return response.SetBadRequest(message: $"Bài học '{lesson.Title}' cần ít nhất 1 tài liệu (material)");
                }

                // Transition Draft → Pending
                await _unitOfWork.BeginTransactionAsync();
                course.Status = 1; // Pending
                course.SubmittedAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;
                course.RejectReason = null;
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.CommitAsync();

                return response.SetOk("Khóa học đã được gửi để duyệt thành công!");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetPendingCoursesForAdminAsync()
        {
            var response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                if (claim.Role != 0)
                {
                    return response.SetBadRequest(message: "Chỉ Admin có quyền xem danh sách chờ duyệt");
                }

                var courses = await _unitOfWork.Courses.GetAllAsync(c => !c.IsDeleted && c.Status == 1);
                var result = courses
                    .OrderByDescending(c => c.SubmittedAt)
                    .Select(c => new PendingCourseReviewResponse
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        Subtitle = c.Subtitle,
                        Image = c.Image,
                        Level = c.Level,
                        Price = c.Price,
                        SubmittedAt = c.SubmittedAt
                    })
                    .ToList();

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        private static string GetMaterialTypeLabel(int type)
        {
            return type switch
            {
                0 => "Video",
                1 => "Reading",
                2 => "Quiz",
                _ => "Material"
            };
        }
    }
}
