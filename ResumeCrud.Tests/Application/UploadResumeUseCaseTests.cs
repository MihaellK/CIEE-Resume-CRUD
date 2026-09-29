using FluentAssertions;
using NSubstitute;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.Tests.Application;

public class UploadResumeUseCaseTests
{
    [Fact]
    public void Execute_ShouldExtractDataAndReturnResume_WhenPdfIsValid()
    {
        // Arrange
        var fakePdfBytes = new byte[] { 1, 2, 3 }; // Simulação de bytes válidos para o mock
        var extractedText = "Nome: Mihaell Alves \n Email: teste@teste.com \n Telefone: 11999999999";
        
        // Usando NSubstitute para mockar o comportamento de extração (não queremos ler arquivo real aqui)
        var mockPdfExtractor = Substitute.For<IPdfTextExtractor>();
        mockPdfExtractor.ExtractText(fakePdfBytes).Returns(extractedText);

        // O Parser nós usamos a instância real, pois não tem dependência externa
        var parserService = new ResumeParserService(); 
        
        var useCase = new UploadResumeUseCase(mockPdfExtractor, parserService);

        // Act
        var result = useCase.Execute("Mihaell Alves", fakePdfBytes);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Mihaell Alves");
        result.Email.Should().Be("teste@teste.com");
        result.Phone.Should().Be("11999999999");
    }
}