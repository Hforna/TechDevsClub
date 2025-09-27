using Career.Domain.Aggregates.JobRoot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Repositories
{
    public interface IJobRepository
    {
        public Task<Job?> GetJobById(Guid id);
    }
}
