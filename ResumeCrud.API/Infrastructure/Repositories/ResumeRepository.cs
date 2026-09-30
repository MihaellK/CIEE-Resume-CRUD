using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;
using ResumeCrud.API.Infrastructure.Data;

namespace ResumeCrud.API.Infrastructure.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly ResumeDbContext _context;

    public ResumeRepository(ResumeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Resume resume)
    {
        await _context.Resumes.AddAsync(resume);
        await _context.SaveChangesAsync();
    }
}