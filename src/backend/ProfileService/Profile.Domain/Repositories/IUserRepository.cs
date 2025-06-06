using Profile.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> UserByEmail(string email);
        public Task<User?> UserByIdentifier(Guid uid);
    }
}
