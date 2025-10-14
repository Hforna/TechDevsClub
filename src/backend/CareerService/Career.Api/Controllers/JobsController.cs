using Career.Api.Filters;
using Career.Application.Requests.Jobs;
using Career.Application.Responses;
using Career.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Career.Api.Controllers
{
    /// <summary>
    /// Controller for managing job postings and applications.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class JobsController : BaseController
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        /// <summary>
        /// Creates a new job posting. Requires authorization to manage jobs.
        /// Only company owners or staff with hiring manager role can create jobs.
        /// </summary>
        /// <param name="request">Job creation data including title, description, requirements, salary, and location type.</param>
        /// <returns>Created job details with unique identifier.</returns>
        /// <response code="201">Job successfully created. Returns the created job details.</response>
        /// <response code="400">Invalid request data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to manage jobs for this company.</response>
        /// <response code="404">Company not found.</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /jobs
        ///     {
        ///         "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "title": "Senior Software Engineer",
        ///         "description": "We are looking for an experienced developer...",
        ///         "salary": {
        ///             "minSalary": 80000,
        ///             "maxSalary": 120000,
        ///             "currency": "USD"
        ///         },
        ///         "jobRequirements": [
        ///             {
        ///                 "skillId": "skill-123",
        ///                 "isMandatory": true,
        ///                 "experienceTime": "Senior"
        ///             }
        ///         ],
        ///         "experience": "Senior",
        ///         "localType": "Remote"
        ///     }
        /// 
        /// </remarks>
        [Authorize(Policy = "ManageJobs")]
        [HttpPost]
        [ProducesResponseType(typeof(JobResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateNewJob([FromBody] CreateJobRequest request)
        {
            var result = await _jobService.CreateJob(request);

            return Created(string.Empty, result);
        }

        /// <summary>
        /// Submits a job application for a specific job posting.
        /// Allows candidates to apply by uploading their resume and specifying desired salary.
        /// </summary>
        /// <param name="request">Application data including resume file (PDF or TXT) and desired salary.</param>
        /// <param name="jobId">The unique identifier of the job to apply for.</param>
        /// <returns>Application details with confirmation.</returns>
        /// <response code="200">Application successfully submitted. Returns application details.</response>
        /// <response code="400">Invalid file format (only PDF and TXT are accepted) or validation errors.</response>
        /// <response code="404">Job posting not found.</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /jobs/{jobId}/applications
        ///     Content-Type: multipart/form-data
        ///     
        ///     resume: [PDF or TXT file]
        ///     desiredSalary: 95000
        /// 
        /// Notes:
        /// - Resume must be in PDF or TXT format
        /// - Hiring managers and company owner will be notified of the new application
        /// - Desired salary is optional
        /// 
        /// </remarks>
        [HttpPost("{jobId}/applications")]
        [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApplyToJob([FromForm] ApplyToJobRequest request, [FromRoute] Guid jobId)
        {
            var result = await _jobService.ApplyToJob(request, jobId);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of applications for a specific job.
        /// Requires authorization to manage jobs. Only accessible by company owners or hiring managers.
        /// </summary>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="perPage">Number of applications per page (default: 10, max: 100).</param>
        /// <param name="page">Page number to retrieve (1-based).</param>
        /// <returns>Paginated list of job applications with status counts.</returns>
        /// <response code="200">Returns paginated applications with statistics.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to view applications for this job.</response>
        /// <response code="404">Job not found.</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /jobs/3fa85f64-5717-4562-b3fc-2c963f66afa6/applications?perPage=10&amp;page=1
        /// 
        /// Response includes:
        /// - List of applications with candidate details
        /// - Total count of applications by status (Applied, Interview, Rejected)
        /// - Pagination information (hasNextPage, hasPreviousPage, etc.)
        /// 
        /// </remarks>
        [Authorize(Policy = "ManageJobs")]
        [HttpGet("{jobId}/applications")]
        [ProducesResponseType(typeof(JobApplicationPaginatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetJobApplications([FromRoute] Guid jobId, [FromQuery] int perPage, [FromQuery] int page)
        {
            var result = await _jobService.GetJobApplications(jobId, perPage, page);

            return Ok(result);
        }

        /// <summary>
        /// Analyzes job applications using AI/automated criteria.
        /// Requires authorization to manage jobs. Allows batch processing of application reviews.
        /// </summary>
        /// <param name="request">Analysis request containing job IDs and criteria for evaluation.</param>
        /// <returns>Success confirmation.</returns>
        /// <response code="200">Applications successfully analyzed and updated.</response>
        /// <response code="400">Invalid request data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have permission to analyze applications.</response>
        /// <response code="404">Job or applications not found.</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /jobs/analyze
        ///     {
        ///         "jobIds": [
        ///             "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///             "7cb92a31-8d3e-4f1a-9e2b-5a8c7d9f1b3e"
        ///         ],
        ///         "expected": {
        ///             "what user expect to this job over application"
        ///         }
        ///     }
        /// 
        /// </remarks>
        [Authorize(Policy = "ManageJobs")]
        [HttpPost("analyze")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AnalyzeJobApplications([FromBody]AnalyzeJobsRequest request)
        {
            await _jobService.AnalyzeJobApplications(request);

            return Ok();
        }
    }
}