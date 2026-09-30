using Microsoft.AspNetCore.Mvc;
using ResumeCrud.API.Application.Services;

namespace ResumeCrud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumesController : ControllerBase
{
    private readonly IUploadResumeUseCase _useCase;

    public ResumesController(IUploadResumeUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string name, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Error = "O ficheiro PDF é obrigatório." });
        }

        if (file.ContentType != "application/pdf")
        {
            return BadRequest(new { Error = "Apenas ficheiros em formato PDF são permitidos." });
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();

        var resume = await _useCase.ExecuteAsync(name, pdfBytes);

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
}