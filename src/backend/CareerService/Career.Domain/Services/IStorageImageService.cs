using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services
{
    public interface IStorageImageService
    {
        public Task UploadCompanyImage(Guid companyId, string imageName, Stream image);
    }
}
