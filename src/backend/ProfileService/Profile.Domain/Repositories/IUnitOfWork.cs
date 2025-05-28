using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface IUnitOfWork
    {
        public IGenericRepository GenericRepository { get; set; }
        public Task Commit(CancellationToken cancellationToken = default);
    }
}
