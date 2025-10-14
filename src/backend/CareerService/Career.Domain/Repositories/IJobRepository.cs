using Career.Domain.Aggregates.JobRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace Career.Domain.Repositories
{
    public interface IJobRepository
    {
        public Task<Job?> GetJobById(Guid id);
        public IPagedList<JobApplication> GetJobApplicationsPaginated(Guid jobId, int perPage, int page);
        public Task<List<JobApplication>?> GetJobApplicationsByIds(List<Guid> ids);
    }
}
