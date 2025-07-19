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
                var result = await _staffService.StaffRequestToCompany(request);

                return Created(string.Empty, result);
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"An error ocurred: {ex.Message}");

                throw;
            }
        }

        [HttpGet("requests/{requestId}")]
        public async Task<IActionResult> GetRequestToStaffStatus([FromRoute]Guid requestId)
        {
            var result = await _staffService.GetStaffRequestStatus(requestId);

            return Ok(result);
        }

        [HttpGet("requests/my")]
        public async Task<IActionResult> GetMyStaffRequests([FromQuery]int perPage, [FromQuery]int page)
        {
            var result = await _staffService.UserStaffRequests(perPage, page);

            if (!result.Requests.Any())
                return NoContent();

            return Ok(result);
        }
    }
}
