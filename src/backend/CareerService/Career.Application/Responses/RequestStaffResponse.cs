﻿using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class RequestStaffResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string RequesterId { get; set; }
        public ERequestStaffStatus Status { get; set; }
        public string Role { get; set; }
    }
}
