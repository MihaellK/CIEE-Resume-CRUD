using Microsoft.AspNetCore.Mvc;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumesController : ControllerBase
{
    private readonly IUploadResumeUseCase _uploadResumeUseCase;
    private readonly IGetResumesUseCase _getResumesUseCase;
    private readonly IParseResumeUseCase _parseResumeUseCase;

    public ResumesController(
        IUploadResumeUseCase uploadResumeUseCase,
        IGetResumesUseCase getResumesUseCase,
        IParseResumeUseCase parseResumeUseCase) // Nova injeção
    {
        _uploadResumeUseCase = uploadResumeUseCase;
        _getResumesUseCase = getResumesUseCase;
        _parseResumeUseCase = parseResumeUseCase;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string name, [FromForm] IFormFile file)
    {
        var validationError = ValidatePdfFile(file);
        if (validationError != null) return validationError;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();

        var resume = await _uploadResumeUseCase.ExecuteAsync(name, pdfBytes);

        var response = new
        {
            resume.Id,
            resume.Name,
            resume.Email,
            resume.Phone
        };

        return Created(string.Empty, response);
    }

    // NOVO ENDPOINT DE EXTRAÇÃO (AUTOFILL)
    [HttpPost("parse")]
    public async Task<IActionResult> Parse([FromForm] IFormFile file)
    {
        var validationError = ValidatePdfFile(file);
        if (validationError != null) return validationError;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();

        var parsedData = _parseResumeUseCase.Execute(pdfBytes);

        return Ok(parsedData); // Retorna os dados extraídos sem guardar no SQL
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var resumes = await _getResumesUseCase.ExecuteAsync();
        return Ok(resumes);
    }

    // Método privado para evitar duplicação de regras de negócio (DRY)
    private IActionResult? ValidatePdfFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails { Title = "Erro de Validação", Detail = "O currículo em PDF é obrigatório." });
        }

        if (file.ContentType != "application/pdf")
        {
            return BadRequest(new ProblemDetails { Title = "Erro de Validação", Detail = "Apenas ficheiros em formato PDF são permitidos." });
        }

        const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxFileSize)
        {
            return BadRequest(new ProblemDetails { Title = "Erro de Validação", Detail = "O ficheiro não pode exceder 5MB." });
        }

        return null;
    }
}