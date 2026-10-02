using FluentAssertions;
using NSubstitute;
using ResumeCrud.API.Application.DTOs;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.Tests.Application;

public class ParseResumeUseCaseTests
{
    [Fact]
    public void Execute_ShouldReturnParsedData_WhenPdfIsValid()
    {
        // Arrange
        var pdfExtractorMock = Substitute.For<IPdfTextExtractor>();
        var parserMock = Substitute.For<IResumeParserService>();

        var fakePdfBytes = new byte[] { 1, 2, 3 };
        var extractedText = "Nome: João Silva\nEmail: joao@teste.com\nTelefone: 11999999999";
        
        pdfExtractorMock.ExtractText(fakePdfBytes).Returns(extractedText);
        
        // Assumindo que o seu parser devolve um objeto com Name, Email e Phone
        parserMock.ParseText(extractedText).Returns(new ParsedResumeDto 
        { 
            Name = "João Silva", 
            Email = "joao@teste.com", 
            Phone = "11999999999" 
        });

        var useCase = new ParseResumeUseCase(pdfExtractorMock, parserMock);

        // Act
        var result = useCase.Execute(fakePdfBytes);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao@teste.com");
        result.Phone.Should().Be("11999999999");

        pdfExtractorMock.Received(1).ExtractText(fakePdfBytes);
        parserMock.Received(1).ParseText(extractedText);
    }
}