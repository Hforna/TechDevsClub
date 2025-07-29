using Career.Api.Filters;
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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCompany([FromRoute]Guid id)
        {
            var result = await _companyService.GetCompany(id);

            return Ok(result);
        }


        /// <summary>
        /// Get all staffs from a company, 
        /// it only will be accepted if user is owner or staff of this company, 
        /// otherwise company owner must set staffs as public in company configurations
        /// </summary>
        /// <param name="perPage">staffs count in response</param>
        /// <param name="page">page number of staffs</param>
        /// <returns>return short data of staffs and infos about the page</returns>
        [HttpGet("{companyId}/staffs")]
        public async Task<IActionResult> GetAllCompanyStaffs([FromRoute]Guid companyId)
        {
            var result = await _companyService.GetCompanyStaffs(companyId);

            if (!result.Staffs.Any())
                return NoContent();

            return Ok(result);
        }

        [UserAuthenticated]
        [HttpPut("{companyId}/configurations")]
        public async Task<IActionResult> UpdateCompanyConfigurations([FromRoute]Guid companyId, 
            [FromBody]CompanyConfigurationRequest request)
        {
            var result = await _companyService.UpdateCompanyConfiguration(companyId, request);

            return Ok();
        }

        [UserAuthenticated]
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody]CreateCompanyRequest request)
        {
            var result = await _companyService.CreateCompany(request);

            return Created(string.Empty, result);
        }

        [UserAuthenticated]
        [HttpPut]
        public async Task<IActionResult> UpdateCompany([FromForm]UpdateCompanyRequest request)
        {
            var result = await _companyService.UpdateCompany(request);

            _logger.LogInformation($"Company updated: {request.CompanyId}");

            return Ok(result);
        }
    }
}
