using FluentAssertions;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.Tests.Application;

public class ResumeParserServiceTests
{
    private readonly ResumeParserService _parserService;

    public ResumeParserServiceTests()
    {
        _parserService = new ResumeParserService();
    }

    [Theory]
    [InlineData("CURRICULUM VITAE\r\nMihaell Alves\r\nDesenvolvedor", "Mihaell Alves")]
    [InlineData("\n\n Currículo \n\nJoão da Silva\njoao@teste.com", "João da Silva")]
    [InlineData("RESUME\nMaria Santos\n(11) 99999-9999", "Maria Santos")]
    [InlineData("Dados Pessoais\nAna Paula\nana@teste.com", "Ana Paula")]
    [InlineData("Carlos Eduardo", "Carlos Eduardo")]
    // Novo cenário: Simulando a perda de quebra de linha (achatamento) do extrator de PDF
    [InlineData("Mihaell Brenno Alves Klosowski Brazil - SP | (11) 98200-2797 | mihaell.klosowski@gmail.com", "Mihaell Brenno Alves Klosowski Brazil")] 
    public void ParseText_ShouldExtractName_IgnoringBoilerplateHeaders(string rawText, string expectedName)
    {
        // Act
        var result = _parserService.ParseText(rawText);

        // Assert
        result.Name.Should().Be(expectedName);
    }

    [Fact]
    public void ParseText_ShouldReturnNullName_WhenTextIsOnlyBoilerplate()
    {
        // Arrange
        var text = "Curriculum Vitae\n \n";

        // Act
        var result = _parserService.ParseText(text);

        // Assert
        result.Name.Should().BeNull();
    }

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

        // Act
        var result = _parserService.ParseText(rawText);

        // Assert
        result.Email.Should().Be("mihaell.alves@teste.com.br");
        result.Phone.Should().Be("(11) 98765-4321");
    }

    [Fact]
    public void Parse_ShouldReturnNullFields_WhenTextDoesNotContainEmailOrPhone()
    {
        // Arrange
        var rawText = "Apenas um texto sem contatos. Desenvolvedor focado em TDD.";

        // Act
        var result = _parserService.ParseText(rawText);

        // Assert
        result.Email.Should().BeNull();
        result.Phone.Should().BeNull();
    }
}