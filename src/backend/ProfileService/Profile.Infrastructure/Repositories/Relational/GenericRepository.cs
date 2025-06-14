using Microsoft.EntityFrameworkCore;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using Profile.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Repositories
{
    public class GenericRepository : IGenericRepository
    {
        private readonly DataContext _dbContext;

        public GenericRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add<T>(T entity) where T : class
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public async Task AddRange<T>(ICollection<T> entities) where T : class
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
        }

        public void Delete<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public void DeleteRange<T>(ICollection<T> entities) where T : class
        {
            _dbContext.Set<T>().RemoveRange(entities);
        }

        public async Task<T?> GetById<T>(long id) where T : class, IEntity
        {
            return await _dbContext.Set<T>().SingleOrDefaultAsync(d => d.Id == id);
        }

        public void Update<T>(T entity) where T : class, IEntity
        {
            _dbContext.Set<T>().Update(entity);
        }
    }
}
