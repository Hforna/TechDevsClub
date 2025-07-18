﻿using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StaffsNumber { get; set; }
        public string? Logo { get; set; }
        public Location Location { get; set; }
        public bool Verified { get; set; }
        public string? Website { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal Rate { get; set; }
        public List<StaffResponse> Staffs { get; set; } = [];
    }
}
