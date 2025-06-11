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
        public IUserRepository UserRepository { get; set; }
        public IProfileRepository ProfileRepository { get; set; }
        public ISkillRespository SkillRepository { get; set; }
        public Task Commit(CancellationToken cancellationToken = default);
    }
}
