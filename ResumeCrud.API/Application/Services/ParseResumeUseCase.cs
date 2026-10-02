using ResumeCrud.API.Application.DTOs;

namespace ResumeCrud.API.Application.Services;

public interface IParseResumeUseCase
{
    ParsedResumeDto Execute(byte[] pdfBytes);
}

public class ParseResumeUseCase : IParseResumeUseCase
{
    private readonly IPdfTextExtractor _pdfExtractor;
    private readonly IResumeParserService _parserService;

    public ParseResumeUseCase(IPdfTextExtractor pdfExtractor, IResumeParserService parserService)
    {
        _pdfExtractor = pdfExtractor;
        _parserService = parserService;
    }

    public ParsedResumeDto Execute(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            throw new ArgumentException("O arquivo PDF é inválido ou está vazio.");

        var extractedText = _pdfExtractor.ExtractText(pdfBytes);
        return _parserService.ParseText(extractedText);
    }
}