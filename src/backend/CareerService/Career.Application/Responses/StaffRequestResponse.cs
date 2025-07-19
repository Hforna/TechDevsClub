using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Responses
{
    public class StaffRequestResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string RequesterId { get; set; }
        public ERequestStaffStatus Status { get; set; }
        public string Role { get; set; }
    }

    public class StaffRequestsResponse
    {
        public string UserId { get; set; }
        public List<StaffRequestResponse> Requests { get; set; }
        public int TotalPending { get; set; }
        public int TotalAccepted { get; set; }
        public int TotalRejected { get; set; }
        public int Count { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
    }
}
