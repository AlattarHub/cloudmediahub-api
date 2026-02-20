using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CloudMediaHub.Api.Configuration;
using CloudMediaHub.Api.Data;
using CloudMediaHub.Api.Data.Entities;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Options;
using System;

namespace CloudMediaHub.Api.Services
{
    public class BlobService
    {
        private readonly AzureStorageSettings _settings;
        private readonly AppDbContext  _db;

        public BlobService(IOptions<AzureStorageSettings> settings, AppDbContext db) {
            _settings = settings.Value;
            _db = db;
        }

        public BlobContainerClient GetContainer()
        {
            var client = new BlobContainerClient(
    new Uri($"https://{_settings.AccountName}.blob.core.windows.net/{_settings.ContainerName}"),
    new DefaultAzureCredential());

            return client;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder = "")
        {
            var container = GetContainer();

            await container.CreateIfNotExistsAsync();

            var extension = Path.GetExtension(file.FileName);

            var fileName = $"{Guid.NewGuid()}{extension}";

            if(!string.IsNullOrWhiteSpace(folder))
            {
                fileName = $"{folder}/{fileName}";
            }

            var blob = container.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();

            await blob.UploadAsync(stream, overwrite: false);

            await saveFileAsync(file, folder, fileName, blob.Uri.ToString());
            return blob.Uri.ToString();
        }
        
        public string GenerateReadSasUrl(string blobName, int expiryMinutes = 10)
        { 
            var blobClient = GetContainer().GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _settings.ContainerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
              



        private async Task saveFileAsync(IFormFile file, string folderName, string blobName, string url)
        {
            var entity = new MediaFile
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                BlobName = blobName,
                Url = url,
                ContentType = file.ContentType,
                Size = file.Length,
                Folder = folderName,
                UploadedAt = DateTime.UtcNow
            };

            _db.MediaFiles.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string blobName)
        {
            var container = GetContainer();

            var blob = container.GetBlobClient(blobName);

            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        
    }
}
