using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Career.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JobsController : BaseController
    {
        
        [Authorize(Policy = "ManageJobs")]
        [HttpPost]
        public async Task<IActionResult> CreateNewJob()
        {
        
        }
    }
}
