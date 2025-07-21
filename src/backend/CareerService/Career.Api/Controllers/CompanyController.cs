using Career.Application.Requests.Company;
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

        [HttpGet("{companyId}/staffs")]
        public async Task<IActionResult> GetAllCompanyStaffs([FromRoute]Guid companyId)
        {
            var result = await _companyService.GetCompanyStaffs(companyId);

            if (!result.Staffs.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody]CreateCompanyRequest request)
        {
            var result = await _companyService.CreateCompany(request);

            return Created(string.Empty, result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompany([FromForm]UpdateCompanyRequest request)
        {
            var result = await _companyService.UpdateCompany(request);

            _logger.LogInformation($"Company updated: {request.CompanyId}");

            return Ok(result);
        }
    }
}
