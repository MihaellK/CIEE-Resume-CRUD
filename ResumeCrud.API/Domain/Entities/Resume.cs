namespace ResumeCrud.API.Domain.Entities;

public class Resume
{
    private const int MaxPdfSizeBytes = 5 * 1024 * 1024; // 5 MB

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Email { get; private set; }
    public byte[] PdfContent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Resume(string name, string? email, byte[] pdfContent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do candidato é obrigatório.", nameof(name));

        if (pdfContent == null || pdfContent.Length == 0)
            throw new ArgumentException("O conteúdo do currículo não pode estar vazio.", nameof(pdfContent));

        if (pdfContent.Length > MaxPdfSizeBytes)
            throw new ArgumentException("O arquivo de currículo não pode exceder 5MB.", nameof(pdfContent));

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PdfContent = pdfContent;
        CreatedAt = DateTime.UtcNow;
    }
}