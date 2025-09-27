using Career.Domain.Entities;
using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Aggregates.CompanyRoot
{
    [Table("reviews")]
    public class Review : IEntity
    {
        public Guid Id { get; set; }
        public string ProfileId { get; set; }
        public Guid CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        public string Text { get; set; }
        public ECompanyRate Rate { get; set; }
    }
}
