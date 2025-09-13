using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain.Services.Messaging
{
    public interface IStaffServiceProducer
    {
        public Task StaffAcceptJoinedCompany(StaffJoinedDto dto);
    }
}
