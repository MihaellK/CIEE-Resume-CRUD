using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Domain.Repositories;

namespace ResumeCrud.API.Application.Services;

public class UploadResumeUseCase : IUploadResumeUseCase
{
    private readonly IPdfTextExtractor _pdfExtractor;
    private readonly ResumeParserService _parserService;
    private readonly IResumeRepository _repository;

    public UploadResumeUseCase(
        IPdfTextExtractor pdfExtractor, 
        ResumeParserService parserService, 
        IResumeRepository repository)
    {
        _pdfExtractor = pdfExtractor;
        _parserService = parserService;
        _repository = repository;
    }

    public async Task<Resume> ExecuteAsync(string name, byte[] pdfBytes)
    {
        // 1. Extrai o texto
        var rawText = _pdfExtractor.ExtractText(pdfBytes);

        // 2. Analisa os dados
        var parsedData = _parserService.ParseText(rawText);

        // 3. Cria a entidade de domínio
        var resume = new Resume(name, parsedData.Email, parsedData.Phone, pdfBytes);

        // 4. Persiste no repositório
        await _repository.AddAsync(resume);

        return resume;
    }
}