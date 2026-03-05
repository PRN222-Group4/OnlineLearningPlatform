using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.MyMapper;
using OnlineLearningPlatform.BusinessObject.Services;
using OnlineLearningPlatform.DataAccess;
using OnlineLearningPlatform.DataAccess.UnitOfWork;
using OnlineLearningPlatform.Presentation.Quartz;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<AppSettings>();
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(config!.ConnectionStrings.DefaultConnection);
    options.EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    options.ConfigureWarnings(warning =>
        warning.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
});

// Add Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ExpirePaymentJob");

    q.AddJob<ExpirePaymentJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithCronSchedule("0 */10 * * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Add Authorization 
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzk2MTY5NjAwIiwiaWF0IjoiMTc2NDY1MzgwMSIsImFjY291bnRfaWQiOiIwMTlhZGQ4ODAxNmE3NDllOTNjNzRjOTE1MTcwM2I0YiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa2JlczA3cDAyODN3c2I3aHNzY25ucmRoIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.FgAdXhDW2NOg4jdFCe0_ybRFo8GseXC6oDmzK1j9SUDDPmX10Dezd_4mItXx7WbaBUcItVN5FW5w-IN0tWFDhsNnC1hQ4ajkzN4Gj8WS2ZCAQNTxKVpCBDXfJbGXWKNc1aZDcbjpE_96_u1xPHEgOnZrDn-V_SNr4PRcQDNVjwP94GF_fJzbBvEsaPwtiOusZEQfEpE80ZdW2_5p9IxOiCirW1S0WYV71gJtq2KBc-O36wUBNPhLiVDmT4SEeRHRwftfLfcuawhK2Ru_hdaJvQocjglZ2YSMFWp67X1uxueRLeiLNV7u9so32QjcjJpg_eH6BAtPQGqKTH_mXTUkiQ", typeof(MapperConfigurationProfile));
builder.Services.AddSingleton(config!);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonResourceService, LessonResourceService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<OnlineLearningPlatform.BusinessObject.IServices.IAdminService, OnlineLearningPlatform.BusinessObject.Services.AdminService>();
builder.Services.AddScoped<IUserLessonProgressService, UserLessonProgressService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Instructor"));
});
// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Teacher", "TeacherOnly");
    options.Conventions.AuthorizeFolder("/Student");
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//controller
app.MapControllers();
//razorpage
app.MapRazorPages();

app.Run();
