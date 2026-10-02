using ResumeCrud.API.Application.DTOs;
using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.API.Application.Services;

public interface ICreateResumeUseCase
{
    Task<Resume> ExecuteAsync(CreateResumeRequestDto request);
}

public class CreateResumeUseCase : ICreateResumeUseCase
{
    private readonly IResumeRepository _repository;

    public CreateResumeUseCase(IResumeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Resume> ExecuteAsync(CreateResumeRequestDto request)
    {
        // A entidade Resume fará as validações de obrigatoriedade (Fail-Fast)
        var resume = new Resume(
            name: request.Name,
            email: request.Email,
            phone: request.Phone,
            areaOfInterest: request.AreaOfInterest,
            professionalSummary: request.ProfessionalSummary,
            pdfContent: null // O PDF já não é guardado neste fluxo
        );

        await _repository.AddAsync(resume);

        return resume;
    }
}