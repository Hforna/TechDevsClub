using Azure.Storage.Blobs;
using Career.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly BlobServiceClient _blobClient;

        public AzureStorageService(BlobServiceClient blobClient)
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

        public async Task UploadUserResumeFile(Guid jobAppId, string resumeName, Stream resume)
        {
            var container = _blobClient.GetBlobContainerClient(jobAppId.ToString());
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlobClient(resumeName);

            await blob.UploadAsync(resume, overwrite: true);
        }

        public Task DeleteCompanyFiles(Guid companyId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteJobsFiles(List<Guid> jobsId)
        {
            throw new NotImplementedException();
        }
    }
}
