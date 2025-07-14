using Career.Domain.Enums;
using Career.Domain.Exceptions;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Aggregates.CompanyRoot
{
    [Table("companies")]
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public string? Logo { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }
        public int EmployeersNumber  { get; set; }
        public bool Verified { get; set; } = false;
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal Rate { get; set; }
        public ICollection<Staff> Staffs { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
    }
}
