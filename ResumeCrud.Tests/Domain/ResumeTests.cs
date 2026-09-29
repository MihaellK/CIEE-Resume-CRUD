using FluentAssertions;
using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.Tests.Domain;

public class ResumeTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsNullOrWhitespace(string? invalidName)
    {
        // Arrange & Act
        Action act = () => new Resume(invalidName, "email@teste.com", null, new byte[] { 0x01 });

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Nome do candidato é obrigatório.*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenPdfContentIsEmpty()
    {
        // Arrange
        var emptyPdf = Array.Empty<byte>();

        // Act
        Action act = () => new Resume("João Silva", "joao@teste.com", null, emptyPdf);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("O conteúdo do currículo não pode estar vazio.*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenPdfContentExceeds5MB()
    {
        // Arrange
        // 5 MB + 1 byte = 5,242,881 bytes
        var oversizedPdf = new byte[5 * 1024 * 1024 + 1]; 

        // Act
        Action act = () => new Resume("João Silva", "joao@teste.com", null, oversizedPdf);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("O arquivo de currículo não pode exceder 5MB.*");
    }
}