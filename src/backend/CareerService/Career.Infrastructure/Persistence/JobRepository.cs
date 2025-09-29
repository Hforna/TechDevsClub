using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace Career.Infrastructure.Persistence
{
    public class JobRepository : IJobRepository
    {
        private readonly DataContext _context;

        public JobRepository(DataContext dataContext)
        {
            _context = dataContext;
        }

        public async Task<Job?> GetJobById(Guid id)
        {
            return await _context.Jobs
                .Include(d => d.Company)
                .SingleOrDefaultAsync(d => d.Id == id);
        }

        public IPagedList<JobApplication> GetJobApplicationsPaginated(Guid jobId, int perPage, int page)
        {
            return _context.JobApplications
                .AsNoTracking()
                .Where(d => d.JobId == jobId)
                .ToPagedList(page, perPage);
        }
    }
}
