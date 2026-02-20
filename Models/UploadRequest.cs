namespace CloudMediaHub.Api.Models
{
    public class UploadRequest
    {
        public List<IFormFile> Files { get; set; }
        public string FolderName { get; set; }
    }
}
