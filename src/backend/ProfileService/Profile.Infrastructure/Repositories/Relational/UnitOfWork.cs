using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DataContext _context;

        public UnitOfWork(DataContext context, IGenericRepository genericRepository, 
            IUserRepository userRepository, IConnectionRepository connectionRepository, 
            IProfileRepository profileRepository, ISkillRespository skillRepository, IDeviceRepository deviceRepository)
        {
            _context = context;
            GenericRepository = genericRepository;
            UserRepository = userRepository;
            ConnectionRepository = connectionRepository;
            ProfileRepository = profileRepository;
            SkillRepository = skillRepository;
            DeviceRepository = deviceRepository;
        }

        public IGenericRepository GenericRepository { get; set; }
        public IUserRepository UserRepository { get; set; }
        public IConnectionRepository ConnectionRepository { get; set; }
        public IProfileRepository ProfileRepository { get; set; }
        public ISkillRespository SkillRepository { get; set; }
        public IDeviceRepository DeviceRepository { get; set; }

        public async Task Commit(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
