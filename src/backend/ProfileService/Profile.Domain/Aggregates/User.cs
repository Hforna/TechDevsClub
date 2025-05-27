using Microsoft.AspNetCore.Identity;
using Profile.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Aggregates
{
    public class User : IdentityUser<long>
    {
        public override Email UserEmail 
        {
            get => Email 
        }
    }
}
