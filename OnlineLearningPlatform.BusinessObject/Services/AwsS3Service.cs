using System.Text.RegularExpressions;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using OnlineLearningPlatform.BusinessObject.IServices;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class AwsS3Service : IStorageService
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        private readonly string _cloudFrontDomain;

        private const string RootFolder = "assets/prn222";

        public AwsS3Service(AppSettings appSettings)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(appSettings.AWS.AccessKey, appSettings.AWS.SecretKey);
            var region = RegionEndpoint.GetBySystemName(appSettings.AWS.Region);

            _s3Client = new AmazonS3Client(credentials, region);
            _bucketName = appSettings.AWS.BucketName;

            _cloudFrontDomain = appSettings.AWS?.CloudFrontDomain.TrimEnd('/') ?? string.Empty;
        }

        public async Task<string> UploadCourseImageAsync(string courseName, IFormFile file)
        {
            var safeCourseName = string.IsNullOrWhiteSpace(courseName) ? "Untitled" : Regex.Replace(courseName, @"[^a-zA-Z0-9\-_]", "-");
            var safeFileName = Regex.Replace(Path.GetFileName(file.FileName), @"[^a-zA-Z0-9.\-_]", "-");
            var key = $"{RootFolder}/Course/{safeCourseName}/{Guid.NewGuid()}_{safeFileName}";
            return await UploadToS3Async(file, key);
        }

        public async Task<string> UploadUserImageAsync(string userName, IFormFile file)
        {
            var safeUserName = string.IsNullOrWhiteSpace(userName) ? "User" : userName.TrimEnd('/').Replace("/", "-").Replace(" ", "-");
            var key = $"{RootFolder}/User/{safeUserName}/{Guid.NewGuid()}_{Path.GetFileName(file.FileName).Replace(" ", "-")}";
            return await UploadToS3Async(file, key);
        }

        public async Task<(string Url, int Type)> UploadLessonResourceAsync(Guid lessonId, string courseName, IFormFile file)
        {
            var safeCourseName = string.IsNullOrWhiteSpace(courseName) ? "Untitled" : courseName.TrimEnd('/').Replace("/", "-").Replace(" ", "-");
            var key = $"{RootFolder}/Courses/{safeCourseName}/Lessons/{lessonId}/{Guid.NewGuid()}_{Path.GetFileName(file.FileName).Replace(" ", "-")}";

            var url = await UploadToS3Async(file, key);
            var resourceType = DetectResourceType(file);

            return (url, resourceType);
        }

        public async Task<string> UploadQuestionSubmissionFileAsync(IFormFile file)
        {
            var key = $"{RootFolder}/QuestionSubmission/ShortAnswer/{Guid.NewGuid()}_{Path.GetFileName(file.FileName).Replace(" ", "-")}";
            return await UploadToS3Async(file, key);
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return false;

            try
            {
                var uri = new Uri(fileUrl);
                var key = uri.AbsolutePath.TrimStart('/');

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> UploadToS3Async(IFormFile file, string key)
        {
            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = key,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return $"{_cloudFrontDomain}/{key}";
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
    }
}