using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Entities
{
    public class Role : IdentityRole<long>, IEntity
    {
        public Role(string role) : base(role) { }
        public Role() : base() { }
    }
}
