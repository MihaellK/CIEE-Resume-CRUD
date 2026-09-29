using System.Text.RegularExpressions;

namespace ResumeCrud.API.Application.Services;

public record ParsedResumeData(string? Email, string? Phone);

public class ResumeParserService
{
    public ParsedResumeData ParseText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedResumeData(null, null);

        // Regex para E-mail
        var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        var emailMatch = emailRegex.Match(rawText);
        var email = emailMatch.Success ? emailMatch.Value : null;

        // Regex para Telefone (Cobre os padrões brasileiros principais: (XX) 9XXXX-XXXX ou XX 9XXXX-XXXX)
        var phoneRegex = new Regex(@"\(?\d{2}\)?\s?(?:9\d{4}|\d{4})[-\s]?\d{4}");
        var phoneMatch = phoneRegex.Match(rawText);
        var phone = phoneMatch.Success ? phoneMatch.Value : null;

        return new ParsedResumeData(email, phone);
    }
}