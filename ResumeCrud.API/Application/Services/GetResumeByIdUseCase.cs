using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.API.Application.Services;

public interface IGetResumeByIdUseCase
{
    Task<Resume?> ExecuteAsync(Guid id);
}

public class GetResumeByIdUseCase : IGetResumeByIdUseCase
{
    private readonly IResumeRepository _repository;

    public GetResumeByIdUseCase(IResumeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Resume?> ExecuteAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
}