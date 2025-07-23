using Career.Domain.Enums;
using Career.Domain.Exceptions;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        public bool Verified { get; set; } = false;
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        private decimal _rate;
        public decimal Rate {
            get => _rate;
            set
            {
                if (value > 5 || value < 0)
                    throw new DomainException(ResourceExceptMessages.COMPANY_RATE_INVALID);

                _rate = value;
            }
        }
        public Guid CompanyConfigurationId { get; set; }
        public CompanyConfiguration CompanyConfiguration { get; set; }
        public ICollection<Staff> Staffs { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
    }

    [Table("companies_configurations")]
    public class CompanyConfiguration
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public bool IsPrivate { get; set; }
        public bool ShowStaffs { get; set; }
        public bool HighlightVerifiedStatus { get; set; } = true;
        public bool NotifyStaffsOnNewReview { get; set; } = false;
        public bool NotifyStaffsOnNewJobApplication { get; set; } = false;
        public bool NotifyStaffsOnJobApplicationUpdate { get; set; } = false;
    }
}
