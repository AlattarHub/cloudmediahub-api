using Microsoft.Extensions.Options;

namespace CloudMediaHub.Api
{
    public interface IFileValidator
    {
        void Validate(IFormFile file);
    }

    

    public class FileValidator : IFileValidator
    {
        private readonly FileValidationSettings _settings;

        public FileValidator(IOptions<FileValidationSettings> settings)
        {
            _settings = settings.Value;
        }

        public void Validate(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new FileValidationException("File is empty");

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!_settings.AllowedExtensions.Contains(extension))
                throw new FileValidationException(
                    $"Extension {extension} is not allowed");

            if (file.Length > _settings.MaxFileSize)
                throw new FileValidationException(
                    $"File size exceeds limit");
        }
    }

    #region Helper Classes
    public class FileValidationSettings
    {
        public string[] AllowedExtensions { get; set; }
        public long MaxFileSize { get; set; }
    }

    public class FileValidationException : Exception
    {
        public FileValidationException(string message)
            : base(message)
        {
        }
    }

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FileValidationException ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
    }
    #endregion

}
