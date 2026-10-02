using FluentAssertions;
using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.Tests.Domain;

public class ResumeTests
{
    [Fact]
    public void Constructor_ShouldCreateValidResume_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        var name = "Mihaell Alves";
        var email = "mihaell@teste.com";
        var phone = "11999999999";
        var areaOfInterest = "Engenharia de Software";
        var professionalSummary = "Especialista em React e .NET com foco em TDD.";

        // Act
        var resume = new Resume(name, email, phone, areaOfInterest, professionalSummary);

        // Assert
        resume.Name.Should().Be(name);
        resume.Email.Should().Be(email);
        resume.AreaOfInterest.Should().Be(areaOfInterest);
        resume.ProfessionalSummary.Should().Be(professionalSummary);
        resume.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsNullOrWhitespace(string? invalidName)
    {
        // Arrange & Act
        Action act = () => new Resume(invalidName!, "email@teste.com", "123", "Area", "Resumo");

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("Nome do candidato é obrigatório.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenEmailIsInvalid(string? invalidEmail)
    {
        // Act
        Action act = () => new Resume("Mihaell", invalidEmail!, "123", "Area", "Resumo");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*e-mail*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenPdfContentExceeds5MB()
    {
        // Arrange
        // 5 MB + 1 byte = 5,242,881 bytes
        var oversizedPdf = new byte[5 * 1024 * 1024 + 1]; 

        // Act
        Action act = () => new Resume("João Silva", "joao@teste.com", null, null, null, oversizedPdf);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("O arquivo de currículo não pode exceder 5MB.*");
    }
}