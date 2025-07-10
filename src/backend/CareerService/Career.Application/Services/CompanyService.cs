using Career.Application.Responses;
using Career.Domain.DomainServices;
using Career.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application.Services
{
    public interface ICompanyService
    {
        public Task<CompanyResponse> CreateCompany()
    }

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CompanyService> _logger;
        private readonly ICompanyDomainService _companyDomain;

    }
}
