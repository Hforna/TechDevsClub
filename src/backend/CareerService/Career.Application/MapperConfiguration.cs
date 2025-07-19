using AutoMapper;
using Career.Application.Requests.Company;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application
{
    public class MapperConfiguration : Profile
    {
        public MapperConfiguration()
        {
            CreateMap<CompanyLocationRequest, Location>();

            CreateMap<CreateCompanyRequest, Company>()
                .ForMember(d => d.Location, f => f.MapFrom(d => d.CompanyLocation));

            CreateMap<UpdateCompanyRequest, Company>()
               .ForMember(d => d.Location, f => f.MapFrom(d => d.CompanyLocation));

            CreateMap<Company, CompanyResponse>()
                .ForMember(d => d.StaffsNumber, f => f.MapFrom(d => d.Staffs.Count));

            CreateMap<RequestStaff, StaffRequestResponse>();
        }
    }
}
