using System.Text.RegularExpressions;
using ResumeCrud.API.Application.DTOs;

namespace ResumeCrud.API.Application.Services;

public class ResumeParserService : IResumeParserService
{
    public ParsedResumeDto ParseText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedResumeDto(); // Retorna um DTO com propriedades a nulo

        // Regex para E-mail
        var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        var emailMatch = emailRegex.Match(rawText);
        var email = emailMatch.Success ? emailMatch.Value : null;

        // Regex para Telefone (Cobre os padrões brasileiros principais)
        var phoneRegex = new Regex(@"\(?\d{2}\)?\s?(?:9\d{4}|\d{4})[-\s]?\d{4}");
        var phoneMatch = phoneRegex.Match(rawText);
        var phone = phoneMatch.Success ? phoneMatch.Value : null;

        // Heurística básica para o Nome: Assume a primeira linha de texto não vazia do PDF
        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var name = lines.FirstOrDefault()?.Trim();

        return new ParsedResumeDto
        {
            Name = name,
            Email = email,
            Phone = phone
        };
    }
}