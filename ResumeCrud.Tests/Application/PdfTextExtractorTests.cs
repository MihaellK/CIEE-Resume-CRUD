using FluentAssertions;
using ResumeCrud.API.Application.Services;
using UglyToad.PdfPig.Core;

namespace ResumeCrud.Tests.Application;

public class PdfTextExtractorTests
{
    [Fact]
    public void ExtractText_ShouldThrowException_WhenByteArrayIsNotAValidPdf()
    {
        // Arrange
        // Criamos um array de bytes que simula um arquivo de texto qualquer disfarçado de PDF
        var fakePdfBytes = System.Text.Encoding.UTF8.GetBytes("Isso não é um PDF de verdade");
        var extractor = new PdfTextExtractorService();

        // Act
        Action act = () => extractor.ExtractText(fakePdfBytes);

        // Assert
        act.Should().Throw<PdfDocumentFormatException>();
    }
}