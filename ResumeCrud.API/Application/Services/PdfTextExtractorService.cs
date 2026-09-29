using System.Text;
using UglyToad.PdfPig;

namespace ResumeCrud.API.Application.Services;

public class PdfTextExtractorService : IPdfTextExtractor
{
    public string ExtractText(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0)
            return string.Empty;

        var textBuilder = new StringBuilder();

        // O método Open do PdfPig já realiza a validação dos 'Magic Numbers' do arquivo.
        // Se não for um PDF válido, ele lançará a PdfDocumentFormatException automaticamente.
        using (var document = PdfDocument.Open(pdfBytes))
        {
            foreach (var page in document.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }
        }

        return textBuilder.ToString();
    }
}