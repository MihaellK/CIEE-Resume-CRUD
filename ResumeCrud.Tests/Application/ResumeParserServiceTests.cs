using FluentAssertions;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.Tests.Application;

public class ResumeParserServiceTests
{
    [Fact]
    public void Parse_ShouldExtractEmailAndPhone_WhenTextContainsValidFormats()
    {
        // Arrange
        var rawText = @"
            CURRÍCULO VITAE
            Nome: Mihaell Alves
            Desenvolvedor Fullstack
            Contato: mihaell.alves@teste.com.br
            Celular: (11) 98765-4321
            Experiência: ...
        ";
        
        var parserService = new ResumeParserService();

        // Act
        var result = parserService.ParseText(rawText);

        // Assert
        result.Email.Should().Be("mihaell.alves@teste.com.br");
        result.Phone.Should().Be("(11) 98765-4321");
    }

    [Fact]
    public void Parse_ShouldReturnNullFields_WhenTextDoesNotContainEmailOrPhone()
    {
        // Arrange
        var rawText = "Apenas um texto sem contatos. Desenvolvedor focado em TDD.";
        var parserService = new ResumeParserService();

        // Act
        var result = parserService.ParseText(rawText);

        // Assert
        result.Email.Should().BeNull();
        result.Phone.Should().BeNull();
    }
}