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
        public IGenericRepository GenericRepository { get; set; }
        public IUserRepository UserRepository { get; set; }

        public UnitOfWork(DataContext context, IGenericRepository genericRepository, IUserRepository userRepository)
        {
            _context = context;
            GenericRepository = genericRepository;
            UserRepository = userRepository;
        }

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
