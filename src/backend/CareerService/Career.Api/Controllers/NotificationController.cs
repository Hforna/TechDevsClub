using Career.Api.Filters;
using Career.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Career.Api.Controllers
{
    [Route("[controller]")]
    [UserAuthenticated]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Retrieves all the user notifications sent using pagination
        /// </summary>
        /// <param name="page">the number page to get the notifications</param>
        /// <param name="perPage">total notification items that user wanna per page</param>
        /// <returns>An ok response containing the notification short details and infos about the page.</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserNotificationPaginated([FromQuery] int page, [FromQuery] int perPage)
        {
            var result = await _notificationService.GetUserNotificationsPaginated(page, perPage);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific notification by its unique identifier for the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to retrieve.</param>
        /// <returns>An Ok response containing the notification details if found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserNotificationById([FromRoute] Guid id)
        {
            var result = await _notificationService.UserNotificationById(id);

            return Ok(result);
        }
    }
}
