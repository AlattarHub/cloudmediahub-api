namespace CloudMediaHub.Api.Data.Entities
{
    public class MediaFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string Folder { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
