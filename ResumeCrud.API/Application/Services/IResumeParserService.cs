using ResumeCrud.API.Application.DTOs;

namespace ResumeCrud.API.Application.Services;

public interface IResumeParserService
{
    ParsedResumeDto ParseText(string text);
}