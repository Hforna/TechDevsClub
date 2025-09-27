using Career.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Repositories
{
    public interface IGenericRepository
    {
        public Task Add<T>(T entity) where T : class;
        public Task AddRange<T>(List<T>? entity) where T : class;
        public void Update<T>(T entity) where T : class;
        public void Remove<T>(T entity) where T : class;
        public Task<T?> GetById<T>(Guid id, bool tracking = true) where T : class, IEntity;
    }
}
