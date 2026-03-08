using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.MyMapper;
using OnlineLearningPlatform.BusinessObject.Services;
using OnlineLearningPlatform.DataAccess;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOnlineLearningPlatformCore(
            this IServiceCollection services,
            AppSettings appSettings)
        {
            var connectionString = appSettings.ConnectionStrings.DefaultConnection;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
                       .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
                options.ConfigureWarnings(warning =>
                    warning.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
            });

            services.AddAutoMapper(cfg => cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzk2MTY5NjAwIiwiaWF0IjoiMTc2NDY1MzgwMSIsImFjY291bnRfaWQiOiIwMTlhZGQ4ODAxNmE3NDllOTNjNzRjOTE1MTcwM2I0YiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa2JlczA3cDAyODN3c2I3aHNzY25ucmRoIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.FgAdXhDW2NOg4jdFCe0_ybRFo8GseXC6oDmzK1j9SUDDPmX10Dezd_4mItXx7WbaBUcItVN5FW5w-IN0tWFDhsNnC1hQ4ajkzN4Gj8WS2ZCAQNTxKVpCBDXfJbGXWKNc1aZDcbjpE_96_u1xPHEgOnZrDn-V_SNr4PRcQDNVjwP94GF_fJzbBvEsaPwtiOusZEQfEpE80ZdW2_5p9IxOiCirW1S0WYV71gJtq2KBc-O36wUBNPhLiVDmT4SEeRHRwftfLfcuawhK2Ru_hdaJvQocjglZ2YSMFWp67X1uxueRLeiLNV7u9so32QjcjJpg_eH6BAtPQGqKTH_mXTUkiQ", typeof(MapperConfigurationProfile));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<HttpClient>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<ILessonService, LessonService>();
            services.AddScoped<ILessonResourceService, LessonResourceService>();
            services.AddScoped<ILessonItemService, LessonItemService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IUserLessonProgressService, UserLessonProgressService>();
            services.AddScoped<IStorageService, AwsS3Service>();
            return services;
        }
    }
}
