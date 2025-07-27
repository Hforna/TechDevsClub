using Career.Api.Filters;
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


        /// <summary>
        /// Owner or staff can request an user to be staff of the selected company.
        /// User will be notified, if they accept, 
        /// they will be at company with the role set on request
        /// </summary>
        /// <param name="request">user id to be requested, 
        /// company id that staff wanna invite the user and the role that user will have if they accept the request</param>
        /// <returns>return infos about the staff request with a status</returns>
        [UserAuthenticated]
        [HttpPost("requests/staff-to-company")]
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

        [UserAuthenticated]
        [HttpGet("requests/{requestId}/accept")]
        public async Task<IActionResult> AcceptRequestToBeStaff([FromRoute]Guid requestId)
        {
            var result = await _staffService.AcceptStaffRequest(requestId);

            return Ok(result);
        }

        [UserAuthenticated]
        [HttpGet("requests/{requestId}/reject")]
        public async Task<IActionResult> RejectStaffRequestToBeStaff([FromRoute]Guid requestId)
        {
            var result = await _staffService.RejectStaffRequest(requestId);

            return Ok(result);
        }

        /// <summary>
        /// return a status of request that user sent to another user
        /// </summary>
        /// <param name="requestId">staff request id</param>
        /// <returns>return status of staff request</returns>
        [UserAuthenticated]
        [HttpGet("requests/{requestId}")]
        public async Task<IActionResult> GetRequestToStaffStatus([FromRoute]Guid requestId)
        {
            var result = await _staffService.GetStaffRequestStatus(requestId);

            return Ok(result);
        }
        
        /// <summary>
        /// If user is a normal or a staff user, 
        /// endpoint'll return all the requestes that other users received to them
        /// </summary>
        /// <param name="perPage">page count</param>
        /// <param name="page">page number</param>
        /// <returns>return infos and status of staff requests</returns>
        [UserAuthenticated]
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
