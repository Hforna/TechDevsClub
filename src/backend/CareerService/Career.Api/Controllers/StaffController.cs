using Career.Application.Requests;
using Career.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Career.Api.Controllers
{
    public class StaffController : BaseController
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        [HttpPost("request-staff-to-company")]
        public async Task<IActionResult> RequestAStaffToCompany([FromBody]PutStaffOnCompanyRequest request)
        {
            try
            {
                var result = await _staffService.RequestStaffToCompany(request);

                return Ok(result);
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"An error ocurred: {ex.Message}");

                throw;
            }
        }
    }
}
