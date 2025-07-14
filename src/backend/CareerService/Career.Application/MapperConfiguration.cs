using AutoMapper;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
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
            CreateMap<Company, CompanyResponse>()
                .ForMember(d => d.StaffsNumber, f => f.MapFrom(d => d.Staffs.Count));
        }
    }
}
