using Career.Application.Requests;
using Career.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Career.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CompanyController : BaseController
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly ICompanyService _companyService;

        public CompanyController(ILogger<CompanyController> logger, ICompanyService companyService)
        {
            _logger = logger;
            _companyService = companyService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody]CreateCompanyRequest request)
        {
            var result = await _companyService.CreateCompany(request);

            return Created(string.Empty, result);
        }
    }
}
