using AutoMapper.Configuration.Conventions;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class CompanyPaginatedResponse
    {
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
        public List<CompanyShortResponse> Companies { get; set; }
        public int Count { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class CompanyShortResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public bool Verified { get; set; }
        public int Rate { get; set; }
    }

    public class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? StaffsNumber { get; set; }
        public string? Logo { get; set; }
        public Location Location { get; set; }
        public bool? Verified { get; set; }
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal Rate { get; set; }
    }

    public class CompanyConfigurationResponse
    {
        public Guid CompanyId { get; set; }
        public bool IsPrivate { get; set; }
        public bool ShowStaffs { get; set; }
        public bool HighlightVerifiedStatus { get; set; } = true;
        public bool NotifyStaffsOnNewReview { get; set; } = false;
        public bool NotifyStaffsOnNewJobApplication { get; set; } = false;
        public bool NotifyStaffsOnJobApplicationUpdate { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
