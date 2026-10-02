using Microsoft.EntityFrameworkCore;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Domain.Repositories;
using ResumeCrud.API.Infrastructure.Data;
using ResumeCrud.API.Infrastructure.Repositories;
using ResumeCrud.API.Infrastructure.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework Core
builder.Services.AddDbContext<ResumeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de Dependências (Infraestrutura)
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();

// Injeção de Dependências (Aplicação)
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractorService>();
builder.Services.AddScoped<IUploadResumeUseCase, UploadResumeUseCase>();
builder.Services.AddScoped<IGetResumesUseCase, GetResumesUseCase>();
builder.Services.AddScoped<IParseResumeUseCase, ParseResumeUseCase>();
builder.Services.AddScoped<IResumeParserService, ResumeParserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Porta do Vite
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();