using AutoMapper;
using Career.Application.Requests.Company;
using Career.Application.Requests.Jobs;
using Career.Application.Responses;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.Dtos;
using Career.Domain.Dtos.Notifications;
using Career.Domain.Entities;
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

            CreateMap<SalaryRequest, Salary>();

            CreateMap<JobRequirementRequest, JobRequirement>();

            CreateMap<CreateJobRequest, Job>().ForMember(d => d.JobRequirements.ToList(), f => f.MapFrom(d => d.JobRequirements));

            CreateMap<ApplyToJobRequest, JobApplication>();

            CreateMap<JobApplication, JobApplicationResponse>();

            CreateMap<Job, JobResponse>();

            CreateMap<JobRequirement, JobRequirementResponse>();

            CreateMap<Salary,  SalaryResponse>();

            CreateMap<Notification, SendNotificationDto>();

            CreateMap<CompanyResponseDto, CompanyResponse>();

            CreateMap<RequestStaff, StaffRequestResponse>();

            CreateMap<CompaniesFilterRequest, CompanyFilterDto>();

            CreateMap<Notification, NotificationResponse>();

            CreateMap<Notification, ShortNotificationResponse>();
        }
    }
}
