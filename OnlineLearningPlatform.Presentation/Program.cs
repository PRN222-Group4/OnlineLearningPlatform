using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.DependencyInjection;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Services;
using OnlineLearningPlatform.Presentation.Quartz;
using OnlineLearningPlatform.Presentation.Hubs;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<AppSettings>();
if (config == null)
{
    throw new InvalidOperationException("AppSettings configuration is missing.");
}

builder.Services.AddScoped<IGradedItemService, GradedItemService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddHttpClient<IAwsAiService, AwsAiService>();
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddOnlineLearningPlatformCore(config);

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ExpirePaymentJob");

    q.AddJob<ExpirePaymentJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithCronSchedule("0 */10 * * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddSingleton(config);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Instructor"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Teacher", "TeacherOnly");
    options.Conventions.AuthorizeFolder("/Student");
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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
app.MapControllers();
app.MapRazorPages();
app.MapHub<RealtimeHub>("/realtimeHub");

app.Run();