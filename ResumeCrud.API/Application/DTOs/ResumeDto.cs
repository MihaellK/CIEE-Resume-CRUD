namespace ResumeCrud.API.Application.DTOs;

public record ResumeDto(Guid Id, string Name, string? Email, string? Phone);