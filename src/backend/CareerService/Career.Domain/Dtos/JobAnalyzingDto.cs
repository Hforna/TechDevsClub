namespace Career.Domain.Dtos;

public record JobAnalyzingDto(string Expected, Guid JobId, List<Guid> JobApplicationsIds);