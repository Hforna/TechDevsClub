using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Services.External
{
    public interface IGitHubService
    {
        public Task<int> GetTotalProfileCommits(string userName);
        public Task<int> GetPublicProfileRepositories(string userName);
    }
}
