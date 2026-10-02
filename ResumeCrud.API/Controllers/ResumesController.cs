using Microsoft.AspNetCore.Mvc;
using ResumeCrud.API.Application.DTOs;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumesController : ControllerBase
{
    private readonly ICreateResumeUseCase _createResumeUseCase;
    private readonly IGetResumesUseCase _getResumesUseCase;
    private readonly IParseResumeUseCase _parseResumeUseCase;
    private readonly IGetResumeByIdUseCase _getResumeByIdUseCase; // 1. Nova dependência

    public ResumesController(
        ICreateResumeUseCase createResumeUseCase,
        IGetResumesUseCase getResumesUseCase,
        IParseResumeUseCase parseResumeUseCase,
        IGetResumeByIdUseCase getResumeByIdUseCase) // 2. Injeção no construtor
    {
        _createResumeUseCase = createResumeUseCase;
        _getResumesUseCase = getResumesUseCase;
        _parseResumeUseCase = parseResumeUseCase;
        _getResumeByIdUseCase = getResumeByIdUseCase;
    }

    // ENDPOINT DE CRIAÇÃO (JSON)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResumeRequestDto request)
    {
        var resume = await _createResumeUseCase.ExecuteAsync(request);

        var response = new
        {
            resume.Id,
            resume.Name,
            resume.Email,
            resume.Phone,
            resume.AreaOfInterest,
            resume.ProfessionalSummary
        };
        
        // Retorna HTTP 201 Created
        return Created(string.Empty, response);
    }

    [HttpPost("parse")]
    public async Task<IActionResult> Parse([FromForm] IFormFile file)
    {
        var validationError = ValidatePdfFile(file);
        if (validationError != null) return validationError;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();

        var parsedData = _parseResumeUseCase.Execute(pdfBytes);

        return Ok(parsedData); 
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var resumes = await _getResumesUseCase.ExecuteAsync();
        return Ok(resumes);
    }

    // 3. NOVO ENDPOINT: Busca por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resume = await _getResumeByIdUseCase.ExecuteAsync(id);

        if (resume == null)
        {
            return NotFound(); // Retorna 404 se não existir
        }

        var response = new
        {
            resume.Id,
            resume.Name,
            resume.Email,
            resume.Phone,
            resume.AreaOfInterest,
            resume.ProfessionalSummary
        };

        return Ok(response); // Retorna 200 com os dados
    }

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