using AutoMapper;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Requests.Enrollment;
using OnlineLearningPlatform.BusinessObject.Requests.Lesson;
using OnlineLearningPlatform.BusinessObject.Requests.LessonResource;
using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Lesson;
using OnlineLearningPlatform.BusinessObject.Responses.LessonResource;
using OnlineLearningPlatform.BusinessObject.Responses.Module;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.BusinessObject.MyMapper
{
    public class MapperConfigurationProfile : Profile
    {
        public MapperConfigurationProfile()
        {
            //User

            //Enrollment
            CreateMap<CreateNewEnrollementRequest, Enrollment>();

            //Course
            CreateMap<CreateNewCourseRequest, Course>();
            CreateMap<Course, CourseResponse>();
            CreateMap<UpdateCourseRequest, Course>();
            CreateMap<Course, GetAllCourseForAdminResponse>();
            CreateMap<Course, StudentCourseDetailResponse>();

            //Lesson
            CreateMap<CreateNewLessonForModuleRequest, Lesson>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Content));
            CreateMap<UpdateLessonRequest, Lesson>();
            CreateMap<Lesson, LessonResponse>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Description));
            CreateMap<Lesson, LessonDetailResponse>();

            //LessonResource
            CreateMap<CreateLessonResourceRequest, LessonResource>();
            CreateMap<LessonResource, LessonResourceResponse>();

            //Module
            CreateMap<CreateNewModuleForCourseRequest, Module>();
            CreateMap<UpdateModuleRequest, Module>();
            CreateMap<Module, ModuleResponse>();

        }
    }
}
