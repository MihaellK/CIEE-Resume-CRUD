namespace ResumeCrud.API.Domain.Entities;

public class Resume
{
    private const int MaxPdfSizeBytes = 5 * 1024 * 1024; // 5 MB

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public string? AreaOfInterest { get; private set; }
    public string? ProfessionalSummary { get; private set; }
    
    // Opcional: podemos guardar o PDF se o utilizador o enviou, 
    // mas não impede o cadastro se for nulo.
    public byte[]? PdfContent { get; private set; } 
    public DateTime CreatedAt { get; private set; }

    // Construtor atualizado com os novos campos
    public Resume(string name, string email, string? phone, string? areaOfInterest, string? professionalSummary, byte[]? pdfContent = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do candidato é obrigatório.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail do candidato é obrigatório.", nameof(email));

        if (pdfContent != null && pdfContent.Length > MaxPdfSizeBytes)
            throw new ArgumentException("O arquivo de currículo não pode exceder 5MB.", nameof(pdfContent));

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        AreaOfInterest = areaOfInterest;
        ProfessionalSummary = professionalSummary;
        PdfContent = pdfContent;
        CreatedAt = DateTime.UtcNow;
    }
}