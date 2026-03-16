using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Requests.Lesson;
using OnlineLearningPlatform.BusinessObject.Requests.Enrollment;
using OnlineLearningPlatform.BusinessObject.Requests.UserLessonProgress;
using OnlineLearningPlatform.BusinessObject.Requests.GradedItem;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.Presentation.Hubs
{
    public class RealtimeHub : Hub
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILessonService _lessonService;
        private readonly IModuleService _moduleService;
        private readonly IUserLessonProgressService _userLessonProgressService;
        private readonly IGradedItemService _gradedItemService;
        private readonly IMessageService _messageService;
        public RealtimeHub(
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            ILessonService lessonService,
            IModuleService moduleService,
            IUserLessonProgressService userLessonProgressService,
            IGradedItemService gradedItemService,
            IMessageService messageService)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _lessonService = lessonService;
            _moduleService = moduleService;
            _userLessonProgressService = userLessonProgressService;
            _gradedItemService = gradedItemService;
            _messageService = messageService;
        }

        // Course Methods
        public async Task<ApiResponse> GetAllCourse()
        {
            var response = await _courseService.GetAllCourseAsync();
            await Clients.Caller.SendAsync("ReceiveAllCourses", response);
            return response;
        }

        public async Task<ApiResponse> GetCourseDetail(Guid courseId)
        {
            var response = await _courseService.GetCourseDetailAsync(courseId);
            await Clients.Caller.SendAsync("ReceiveCourseDetail", response);
            return response;
        }

        public async Task<ApiResponse> GetAllCourseForAdmin(int status)
        {
            var response = await _courseService.GetAllCourseForAdminAsync(status);
            await Clients.Caller.SendAsync("ReceiveAllCoursesForAdmin", response);
            return response;
        }

        public async Task<ApiResponse> GetCoursesByInstructor()
        {
            var response = await _courseService.GetCoursesByInstructorAsync();
            await Clients.Caller.SendAsync("ReceiveCoursesByInstructor", response);
            return response;
        }

        public async Task<ApiResponse> GetEnrolledCoursesForStudent()
        {
            var response = await _courseService.GetEnrolledCoursesForStudentAsync();
            await Clients.Caller.SendAsync("ReceiveEnrolledCourses", response);
            return response;
        }

        public async Task<ApiResponse> GetCoursesByStatus(int status)
        {
            var response = await _courseService.GetCoursesByStatusAsync(status);
            await Clients.Caller.SendAsync("ReceiveCoursesByStatus", response);
            return response;
        }

        public async Task<ApiResponse> GetCourseDetailForStudent(Guid courseId)
        {
            var response = await _courseService.GetCourseDetailForStudentAsync(courseId);
            await Clients.Caller.SendAsync("ReceiveCourseDetailForStudent", response);
            return response;
        }

        public async Task<ApiResponse> GetCourseForLearning(Guid courseId)
        {
            var response = await _courseService.GetCourseForLearningAsync(courseId);
            await Clients.Caller.SendAsync("ReceiveCourseForLearning", response);
            return response;
        }

        public async Task<ApiResponse> GetFilteredCourses(CourseFilterRequest request)
        {
            var response = await _courseService.GetFilteredCoursesAsync(request);
            await Clients.Caller.SendAsync("ReceiveFilteredCourses", response);
            return response;
        }

        public async Task<ApiResponse> GetInstructorMetrics()
        {
            var response = await _courseService.GetInstructorMetricsAsync();
            await Clients.Caller.SendAsync("ReceiveInstructorMetrics", response);
            return response;
        }

        public async Task<ApiResponse> GetPendingCoursesForAdmin()
        {
            var response = await _courseService.GetPendingCoursesForAdminAsync();
            await Clients.Caller.SendAsync("ReceivePendingCourses", response);
            return response;
        }

        // Module Methods
        public async Task<ApiResponse> GetModulesByCourse(Guid courseId)
        {
            var response = await _moduleService.GetModulesByCourseAsync(courseId);
            await Clients.Caller.SendAsync("ReceiveModulesByCourse", response);
            return response;
        }

        public async Task<ApiResponse> GetModuleDetail(Guid moduleId)
        {
            var response = await _moduleService.GetModuleDetailAsync(moduleId);
            await Clients.Caller.SendAsync("ReceiveModuleDetail", response);
            return response;
        }

        // Lesson Methods
        public async Task<ApiResponse> GetLessonsByModule(Guid moduleId)
        {
            var response = await _lessonService.GetLessonsByModuleAsync(moduleId);
            await Clients.Caller.SendAsync("ReceiveLessonsByModule", response);
            return response;
        }

        public async Task<ApiResponse> GetLessonDetail(Guid lessonId)
        {
            var response = await _lessonService.GetLessonDetailAsync(lessonId);
            await Clients.Caller.SendAsync("ReceiveLessonDetail", response);
            return response;
        }

        // Enrollment Methods
        public async Task<ApiResponse> GetStudentEnrollments()
        {
            var response = await _enrollmentService.GetStudentEnrollmentsAsync();
            await Clients.Caller.SendAsync("ReceiveStudentEnrollments", response);
            return response;
        }

        public async Task<ApiResponse> CheckEnrollment(Guid courseId)
        {
            var isEnrolled = await _enrollmentService.CheckEnrollmentAsync(courseId);
            await Clients.Caller.SendAsync("ReceiveEnrollmentStatus", isEnrolled);
            return new ApiResponse { IsSuccess = true, Result = isEnrolled };
        }

        public async Task<ApiResponse> CheckUserEnrollment(Guid userId, Guid courseId)
        {
            var response = await _enrollmentService.CheckUserEnrollmentAsync(userId, courseId);
            await Clients.Caller.SendAsync("ReceiveUserEnrollmentStatus", response);
            return response;
        }

        // User Lesson Progress Methods
        public async Task<ApiResponse> GetLessonProgress(Guid lessonId)
        {
            var response = await _userLessonProgressService.GetLessonProgressAsync(lessonId);
            await Clients.Caller.SendAsync("ReceiveLessonProgress", response);
            return response;
        }

        public async Task<ApiResponse> GetLessonProgressByUser(Guid userId)
        {
            var response = await _userLessonProgressService.GetLessonProgressByUserAsync(userId);
            await Clients.Caller.SendAsync("ReceiveLessonProgressByUser", response);
            return response;
        }

        public async Task<ApiResponse> UpdateProgress(UpdateUserLessonProgressRequest request)
        {
            var response = await _userLessonProgressService.StartOrUpdateProgressAsync(request);
            await Clients.Caller.SendAsync("ReceiveProgressUpdate", response);
            // Notify other users in the same course about progress update
            await Clients.Others.SendAsync("UserProgressUpdated", new { request.LessonId, request.CompletionPercent });
            return response;
        }

        public async Task<ApiResponse> MarkLessonCompleted(Guid lessonId)
        {
            var response = await _userLessonProgressService.MarkLessonCompletedAsync(lessonId);
            await Clients.Caller.SendAsync("ReceiveLessonCompleted", response);
            // Notify others about completion
            await Clients.Others.SendAsync("UserCompletedLesson", lessonId);
            return response;
        }

        // Graded Item Methods
        public async Task<ApiResponse> SubmitQuiz(SubmitQuizRequest request)
        {
            var response = await _gradedItemService.SubmitQuizAsync(request);
            await Clients.Caller.SendAsync("ReceiveQuizSubmission", response);
            return response;
        }

        // Connection Methods
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.Others.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        // Group Management for Course-specific updates
        public async Task JoinCourseGroup(string courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"course_{courseId}");
            await Clients.Group($"course_{courseId}").SendAsync("UserJoinedCourse", Context.ConnectionId);
        }

        public async Task LeaveCourseGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"course_{courseId}");
            await Clients.Group($"course_{courseId}").SendAsync("UserLeftCourse", Context.ConnectionId);
        }

        // Broadcast Methods (for notifications to groups)
        public async Task NotifyCourseUpdate(Guid courseId, object updateData)
        {
            await Clients.Group($"course_{courseId}").SendAsync("CourseUpdated", updateData);
        }

        public async Task NotifyNewEnrollment(Guid courseId, object enrollmentData)
        {
            await Clients.Group($"course_{courseId}").SendAsync("NewEnrollment", enrollmentData);
        }
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }
        public async Task JoinWalletGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"wallet_{userId}");
        }

        // Instructor gọi method này khi submit course
        public async Task NotifyNewPendingCourse(object courseData)
        {
            await Clients.Group("admins").SendAsync("NewPendingCourse", courseData);
        }

        public async Task NotifyLessonProgressUpdate(Guid lessonId, object progressData)
        {
            await Clients.All.SendAsync("LessonProgressUpdated", new { lessonId, progressData });
        }

        public async Task RegisterUserConnection(Guid userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }
        public async Task SendPrivateMessage(Guid senderId, Guid receiverId, string content)
        {
            var response = await _messageService.SendMessageAsync(senderId, receiverId, content);

            if (response.IsSuccess && response.Result != null)
            {
                await Clients.Group(receiverId.ToString()).SendAsync("ReceivePrivateMessage", response.Result);

                await Clients.Caller.SendAsync("ReceivePrivateMessage", response.Result);
            }
        }
    }
}
