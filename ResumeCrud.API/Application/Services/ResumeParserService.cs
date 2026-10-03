using System.Text.RegularExpressions;
using ResumeCrud.API.Application.DTOs;

namespace ResumeCrud.API.Application.Services;

public class ResumeParserService : IResumeParserService
{
    // Lista de cabeçalhos genéricos que devem ser ignorados na busca pelo Nome
    private readonly string[] _boilerplateHeaders = new[]
    {
        "curriculum vitae", "curriculum", "curriculo", "currículo",
        "resume", "dados pessoais", "perfil profissional", "cv"
    };

    // Delimitadores comuns usados após o nome em PDFs achatados
    private readonly char[] _lineDelimiters = new[] { '|', '-', '–', ',', '(', '@' };

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

        // Heurística aprimorada para o Nome
        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string? name = null;

        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            
            // Ignora linhas puramente vazias ou muito curtas
            if (string.IsNullOrWhiteSpace(cleanLine) || cleanLine.Length <= 2) 
                continue;

            // Verifica se a linha é apenas um cabeçalho genérico
            bool isBoilerplate = _boilerplateHeaders.Any(h => 
                cleanLine.Equals(h, StringComparison.InvariantCultureIgnoreCase));

            if (isBoilerplate) continue;

            // 1. Corte por Delimitadores (Evita engolir telefone, email e moradas)
            int delimiterIndex = cleanLine.IndexOfAny(_lineDelimiters);
            if (delimiterIndex > 0)
            {
                cleanLine = cleanLine.Substring(0, delimiterIndex).Trim();
            }

            // 2. Limite de Palavras (Segurança extra contra parágrafos longos sem delimitadores)
            var words = cleanLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 6)
            {
                cleanLine = string.Join(" ", words.Take(6));
            }

            // Valida novamente após a limpeza
            if (cleanLine.Length > 2)
            {
                name = cleanLine;
                break; 
            }
        }

        return new ParsedResumeDto
        {
            Name = name,
            Email = email,
            Phone = phone
        };
    }
}