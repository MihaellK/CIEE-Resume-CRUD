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
builder.Services.AddScoped<ResumeParserService>();
builder.Services.AddScoped<UploadResumeUseCase>();
builder.Services.AddScoped<IUploadResumeUseCase, UploadResumeUseCase>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();