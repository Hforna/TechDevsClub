using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Entities
{
    public interface IEntity
    {
        public Guid Id { get; set; }
    }

    public abstract class Entity
    {
        
    }
}
