using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.API.Domain.Repositories;

public interface IResumeRepository
{
    Task AddAsync(Resume resume);
}