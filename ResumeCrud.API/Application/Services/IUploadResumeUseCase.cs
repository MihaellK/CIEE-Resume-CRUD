using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.API.Application.Services;

public interface IUploadResumeUseCase
{
    Task<Resume> ExecuteAsync(string name, byte[] pdfBytes);
}