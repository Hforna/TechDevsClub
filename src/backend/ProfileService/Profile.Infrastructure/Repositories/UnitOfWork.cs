using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly DataContext _context;

        public UnitOfWork(DataContext context, IGenericRepository genericRepository, 
            IUserRepository userRepository, IProfileRepository profileRepository, ISkillRespository skillRespository)
        {
            _context = context;
            SkillRepository = skillRespository;
            GenericRepository = genericRepository;
            UserRepository = userRepository;
            ProfileRepository = profileRepository;
        }

        public IGenericRepository GenericRepository { get; set; }
        public IUserRepository UserRepository { get; set; }
        public IProfileRepository ProfileRepository { get; set; }
        public ISkillRespository SkillRepository { get; set; }

        public async Task Commit(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
