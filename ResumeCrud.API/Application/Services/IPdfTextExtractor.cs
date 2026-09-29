namespace ResumeCrud.API.Application.Services;

public interface IPdfTextExtractor
{
    string ExtractText(byte[] pdfBytes);
}