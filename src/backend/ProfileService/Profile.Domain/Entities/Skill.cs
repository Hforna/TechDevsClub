using Profile.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Domain.Entities
{
    [Table("skills")]
    public class Skill : IEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public static Skill Create(string name)
        {
            return new Skill() { Name = name };
        }
    }
}
