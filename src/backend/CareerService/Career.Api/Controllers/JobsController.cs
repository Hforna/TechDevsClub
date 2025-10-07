using Career.Api.Filters;
using Career.Application.Requests.Jobs;
using Career.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Career.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JobsController : BaseController
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [Authorize(Policy = "ManageJobs")]
        [HttpPost]
        public async Task<IActionResult> CreateNewJob([FromBody]CreateJobRequest request)
        {
            var result = await _jobService.CreateJob(request);

            return Created(string.Empty, result);
        }


        [HttpPost("{jobId}/applications")]
        public async Task<IActionResult> ApplyToJob([FromForm]ApplyToJobRequest request, [FromRoute]Guid jobId)
        {
            var result = await _jobService.ApplyToJob(request, jobId);

            return Ok(result);
        }

        [Authorize(Policy = "ManageJobs")]
        [HttpGet("{jobId}/applications")]
        public async Task <IActionResult> GetJobApplications([FromRoute]Guid jobId, [FromQuery]int perPage, [FromQuery]int page)
        {
            var result = await _jobService.GetJobApplications(jobId, perPage, page);

            return Ok(result);
        }
    }
}
