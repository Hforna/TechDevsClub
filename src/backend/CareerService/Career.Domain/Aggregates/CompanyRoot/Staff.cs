using Career.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Aggregates.CompanyRoot
{
    [Table("Staffs")]
    public class Staff
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }

    [Table("staff_roles")]
    public class StaffRole
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public Guid CompanyId { get; set; }
        public string Role { get; set; }
    }

    [Table("requests_staffs")]
    public class RequestStaff
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string RequesterId { get; set; }
        public string Role { get; set; }
        public ERequestStaffStatus Status { get; set; }
    }
}
