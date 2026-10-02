namespace ResumeCrud.API.Application.DTOs;

public class CreateResumeRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AreaOfInterest { get; set; }
    public string? ProfessionalSummary { get; set; }
}