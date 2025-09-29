using Career.Api.Filters;
using Career.Application.Requests.Company;
using Career.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

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

        /// <summary>
        /// Retrieves a company by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the company.</param>
        /// <returns>The company details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var result = await _companyService.GetCompany(id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of companies based on filter criteria.
        /// </summary>
        /// <param name="request">Filter request containing search criteria and pagination info.</param>
        /// <returns>Paginated list of companies.</returns>
        [HttpPost("filter")]
        public async Task<IActionResult> GetCompaniesPaginated([FromBody] CompaniesFilterRequest request)
        {
            var result = await _companyService.GetCompanyFiltered(request);
            return Ok(result);
        }

        /// <summary>
        /// Gets all staff from a company. Accessible only if the user is owner or staff, 
        /// otherwise public staff visibility is required.
        /// </summary>
        /// <param name="companyId">ID of the company.</param>
        /// <returns>Short data of staff with pagination info or NoContent if none.</returns>
        [HttpGet("{companyId}/staffs")]
        public async Task<IActionResult> GetAllCompanyStaffs(Guid companyId)
        {
            var result = await _companyService.GetCompanyStaffs(companyId);

            if (!result.Staffs.Any())
                return NoContent();

            return Ok(result);
        }

        /// <summary>
        /// Updates the company's configurations. Requires authenticated user.
        /// </summary>
        /// <param name="companyId">ID of the company.</param>
        /// <param name="request">Configuration data to update.</param>
        /// <returns>Updated configuration info.</returns>
        [UserAuthenticated]
        [HttpPut("{companyId}/configurations")]
        public async Task<IActionResult> UpdateCompanyConfigurations(Guid companyId, [FromBody] CompanyConfigurationRequest request)
        {
            var result = await _companyService.UpdateCompanyConfiguration(companyId, request);
            return Ok();
        }

        /// <summary>
        /// Retrieves company configuration info. Requires authenticated owner.
        /// </summary>
        /// <param name="companyId">ID of the company.</param>
        /// <returns>Company configuration details.</returns>
        [UserAuthenticated]
        [HttpGet("{companyId}/configurations")]
        [Authorize("OnlyOwner")]
        public async Task<IActionResult> GetCompanyConfigurations(Guid companyId)
        {
            var result = await _companyService.GetCompanyConfigurationInfos(companyId);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new company. Requires authenticated user.
        /// </summary>
        /// <param name="request">Data to create the company.</param>
        /// <returns>Created company details.</returns>
        [Authorize(Policy = "OnlyOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
        {
            var result = await _companyService.CreateCompany(request);
            return Created(string.Empty, result);
        }

        /// <summary>
        /// Updates an existing company. Requires authenticated user.
        /// </summary>
        /// <param name="request">Data to update the company.</param>
        /// <returns>Updated company details.</returns>
        [Authorize(Policy = "OnlyOwner")]
        [HttpPut]
        public async Task<IActionResult> UpdateCompany([FromForm] UpdateCompanyRequest request)
        {
            var result = await _companyService.UpdateCompany(request);
            _logger.LogInformation($"Company updated: {request.CompanyId}");
            return Ok(result);
        }

        /// <summary>
        /// Removes a staff member from a company. Requires owner authorization.
        /// </summary>
        /// <param name="companyId">ID of the company.</param>
        /// <param name="staffId">ID of the staff to remove.</param>
        /// <param name="reason">Reason for removal.</param>
        /// <returns>Success status.</returns>
        [Authorize(Policy = "OnlyOwner")]
        [HttpDelete("{companyId}/staffs/{staffId}")]
        public async Task<IActionResult> FireStaffFromCompany(Guid companyId, Guid staffId, string reason)
        {
            await _companyService.FireStaffFromCompany(companyId, staffId, reason);
            return Ok();
        }
    }
}
