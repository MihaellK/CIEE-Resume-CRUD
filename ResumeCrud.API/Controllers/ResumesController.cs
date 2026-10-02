using Microsoft.AspNetCore.Mvc;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumesController : ControllerBase
{
    private readonly IUploadResumeUseCase _uploadResumeUseCase;
    private readonly IGetResumesUseCase _getResumesUseCase;

    public ResumesController(
        IUploadResumeUseCase uploadResumeUseCase,
        IGetResumesUseCase getResumesUseCase)
    {
        _uploadResumeUseCase = uploadResumeUseCase;
        _getResumesUseCase = getResumesUseCase;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string name, [FromForm] IFormFile file)
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

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();

        var resume = await _uploadResumeUseCase.ExecuteAsync(name, pdfBytes);

        // Devolvemos um DTO anónimo para não expor os bytes do ficheiro no JSON de resposta
        var response = new
        {
            resume.Id,
            resume.Name,
            resume.Email,
            resume.Phone
        };

        return Created(string.Empty, response);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var resumes = await _getResumesUseCase.ExecuteAsync();
        return Ok(resumes); // Retorna HTTP 200 com o array de ResumeDto
    }
}