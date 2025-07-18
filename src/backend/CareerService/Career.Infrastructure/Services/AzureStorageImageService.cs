using Azure.Storage.Blobs;
using Career.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Services
{
    public class StorageImageService : IStorageImageService
    {
        private readonly BlobServiceClient _blobClient;

        public StorageImageService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task UploadCompanyImage(Guid companyId, string imageName, Stream image)
        {
            var container = _blobClient.GetBlobContainerClient(companyId.ToString());
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlobClient(imageName);

            await blob.UploadAsync(image, overwrite: true);
        }
    }
}
