using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services
{
    public interface IStorageService
    {
        public Task UploadCompanyImage(Guid companyId, string imageName, Stream image);
        public Task UploadUserResumeFile(Guid jobAppId, string resumeName, Stream resume);
        public Task DeleteCompanyFiles(Guid companyId);
        public Task DeleteJobsFiles(List<Guid> jobsId);
    }
}
