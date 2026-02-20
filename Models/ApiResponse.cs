namespace CloudMediaHub.Api.Models
{

    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public List<string> Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailResponse(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }


    public class UploadResult
    {
        public List<string> Uploaded { get; set; } = new();
        public List<FileError> Failed { get; set; } = new();
    }

    public class FileError
    {
        public string FileName { get; set; }
        public string Error { get; set; }
    }


}
