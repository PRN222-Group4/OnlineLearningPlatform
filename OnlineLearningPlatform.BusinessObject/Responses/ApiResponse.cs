using System.Net;

namespace OnlineLearningPlatform.BusinessObject.Responses
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public object Result { get; set; }

        public ApiResponse SetOk(object result = null)
        {
            IsSuccess = true;
            StatusCode = HttpStatusCode.OK;
            Result = result;
            return this;
        }

        public ApiResponse SetNotFound(object result = null, string message = null)
        {
            IsSuccess = false;
            StatusCode = HttpStatusCode.NotFound;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }

        public ApiResponse SetBadRequest(object result = null, string message = null)
        {
            IsSuccess = false;
            StatusCode = HttpStatusCode.BadRequest;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }

        public ApiResponse SetApiResponse(HttpStatusCode statusCode, bool isSuccess, string message = null, object result = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage = message;
            }
            Result = result;
            return this;
        }
    }
}
