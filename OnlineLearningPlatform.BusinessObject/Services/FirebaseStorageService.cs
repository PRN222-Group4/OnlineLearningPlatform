using OnlineLearningPlatform.BusinessObject.IServices;
using Microsoft.AspNetCore.Http;
using Firebase.Storage;
using Microsoft.Extensions.Configuration;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly IConfiguration _config;

        public FirebaseStorageService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadCourseImage(string courseName, IFormFile? file)
        {
            string firebaseBucket = _config["Firebase:Bucket"];

            var firebaseStorage = new FirebaseStorage(firebaseBucket);

            string fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(file.FileName)}";

            if (courseName.EndsWith("/"))
            {
                courseName = courseName.TrimEnd('/');
            }

            fileName = fileName.Replace("/", "-");

            var task = firebaseStorage.Child("Course").Child(courseName).Child(fileName);

            var stream = file.OpenReadStream();
            await task.PutAsync(stream);

            return await task.GetDownloadUrlAsync();
        }

        public async Task<string> UploadUserImage(string userName, IFormFile? file)
        {
            string firebaseBucket = _config["Firebase:Bucket"];

            var firebaseStorage = new FirebaseStorage(firebaseBucket);

            string fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(file.FileName)}";

            if (userName.EndsWith("/"))
            {
                userName = userName.TrimEnd('/');
            }

            fileName = fileName.Replace("/", "-");

            var task = firebaseStorage.Child("User").Child(userName).Child(fileName);

            var stream = file.OpenReadStream();
            await task.PutAsync(stream);

            return await task.GetDownloadUrlAsync();
        }

        public async Task<(string Url, int Type)> UploadLessonResourceAsync(
           Guid lessonId,
           string courseName,
           IFormFile file)
        {
            string bucket = _config["Firebase:Bucket"];
            var storage = new FirebaseStorage(bucket);

            var resourceType = DetectResourceType(file);

            string safeCourseName = courseName.TrimEnd('/').Replace("/", "-");
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}"
                                .Replace("/", "-");

            var task = storage
                .Child("Courses")
                .Child(safeCourseName)
                .Child("Lessons")
                .Child(lessonId.ToString())
                .Child(fileName);

            await using var stream = file.OpenReadStream();
            await task.PutAsync(stream);

            var url = await task.GetDownloadUrlAsync();
            return (url, resourceType);
        }
        private int DetectResourceType(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            return ext switch
            {
                ".mp4" or ".mov" or ".avi" or ".mkv" => 0,
                ".mp3" or ".wav" => 6,
                ".jpg" or ".jpeg" or ".png" or ".gif" => 5,
                ".pdf" => 1,
                ".ppt" or ".pptx" => 2,
                ".doc" or ".docx" => 3,
                _ => 4
            };
        }

        public async Task<string> UploadQuestionSubmissionFile(IFormFile? file)
        {
            string firebaseBucket = _config["Firebase:Bucket"];

            var firebaseStorage = new FirebaseStorage(firebaseBucket);

            string fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(file.FileName)}";

            fileName = fileName.Replace("/", "-");

            var task = firebaseStorage.Child("QuestionSubmission").Child("ShortAnswer").Child(fileName);

            var stream = file.OpenReadStream();
            await task.PutAsync(stream);

            return await task.GetDownloadUrlAsync();
        }
    }
}
