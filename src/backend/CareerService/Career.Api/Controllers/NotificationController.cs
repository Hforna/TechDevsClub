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

        [HttpGet]
        public async Task<IActionResult> GetUserNotificationPaginated([FromQuery] int page, [FromQuery] int perPage)
        {
            var result = await _notificationService.GetUserNotificationsPaginated(page, perPage);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserNotificationById([FromRoute] Guid id)
        {
            var result = await _notificationService.UserNotificationById(id);

            return Ok(result);
        }
    }
}
