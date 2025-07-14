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
    }
}
