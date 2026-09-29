using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.API.Application.Services;

public class UploadResumeUseCase
{
    private readonly IPdfTextExtractor _pdfExtractor;
    private readonly ResumeParserService _parserService;

    public UploadResumeUseCase(IPdfTextExtractor pdfExtractor, ResumeParserService parserService)
    {
        _pdfExtractor = pdfExtractor;
        _parserService = parserService;
    }

    public Resume Execute(string name, byte[] pdfBytes)
    {
        // 1. Extrai o texto bruto do PDF utilizando o serviço injetado
        var rawText = _pdfExtractor.ExtractText(pdfBytes);

        // 2. Faz o parsing do texto para extrair metadados (email, telefone, etc.)
        var parsedData = _parserService.ParseText(rawText);

        // 3. Instancia a entidade de Domínio validando as regras de negócio essenciais
        var resume = new Resume(name, parsedData.Email, parsedData.Phone, pdfBytes);

        return resume;
    }
}