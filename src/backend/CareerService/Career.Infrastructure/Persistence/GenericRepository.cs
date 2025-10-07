using Career.Domain.Entities;
using Career.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure.Persistence
{
    public class GenericRepository : IGenericRepository
    {
        private readonly DataContext _context;

        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        public async Task Add<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task AddRange<T>(List<T>? entity) where T : class
        {
            await _context.AddRangeAsync(entity);
        }

        public void DeleteRange<T>(List<T> entity) where T : class
        {
            _context.Set<T>().RemoveRange(entity);
        }

        public async Task<T?> GetById<T>(Guid id, bool tracking = true) where T : class, IEntity
        {
            return tracking 
                ? await _context.Set<T>().SingleOrDefaultAsync(d => d.Id == id) 
                : await _context.Set<T>().AsNoTracking().SingleOrDefaultAsync(d => d.Id == id);
        }

        public void Remove<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
        }
    }
}
