using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Repositories
{
    public interface IGenericRepository
    {
        public Task<T?> GetById<T>(long id) where T : class, IEntity;
        public void Delete<T>(T entity) where T : class;
        public Task Add<T>(T entity) where T : class;
        public Task AddRange<T>(ICollection<T> entities) where T : class; 
        public void DeleteRange<T>(ICollection<T> entities) where T : class;
    }
}
