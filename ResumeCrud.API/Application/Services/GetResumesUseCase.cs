using ResumeCrud.API.Application.DTOs;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.API.Application.Services;

public interface IGetResumesUseCase
{
    Task<IEnumerable<ResumeDto>> ExecuteAsync();
}

public class GetResumesUseCase : IGetResumesUseCase
{
    private readonly IResumeRepository _repository;

    public GetResumesUseCase(IResumeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ResumeDto>> ExecuteAsync()
    {
        var resumes = await _repository.GetAllAsync();
        
        // Mapeamos a Entidade para DTO para não expor/trafegar os bytes do PDF na API
        return resumes.Select(r => new ResumeDto(r.Id, r.Name, r.Email, r.Phone));
    }
}