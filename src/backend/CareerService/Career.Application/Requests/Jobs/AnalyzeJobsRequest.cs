namespace Career.Application.Requests.Jobs;

public class AnalyzeJobsRequest
{
    public string? Expected { get; set; }
    public Guid JobId { get; set; }
    public List<Guid> JobApplicationsIds { get; set; }
}