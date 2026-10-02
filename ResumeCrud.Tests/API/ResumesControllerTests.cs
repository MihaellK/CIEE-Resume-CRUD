using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ResumeCrud.API.Application.Services;
using ResumeCrud.API.Controllers;
using ResumeCrud.API.Domain.Entities;
using ResumeCrud.API.Application.DTOs;

namespace ResumeCrud.Tests.API;

public class ResumesControllerTests
{
    [Fact]
    public async Task Create_ShouldReturnCreatedResult_WhenDataIsValid()
    {
        // Arrange
        var createUseCaseMock = Substitute.For<ICreateResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();
        var parseUseCaseMock = Substitute.For<IParseResumeUseCase>();
        var getByIdUseCaseMock = Substitute.For<IGetResumeByIdUseCase>(); 

        var requestDto = new CreateResumeRequestDto
        {
            Name = "Mihaell Alves",
            Email = "email@teste.com",
            Phone = "11999999999",
            AreaOfInterest = "Engenharia de Software"
        };

        var expectedResume = new Resume(requestDto.Name, requestDto.Email, requestDto.Phone, requestDto.AreaOfInterest, null, null);
        
        createUseCaseMock.ExecuteAsync(requestDto).Returns(expectedResume);

        var controller = new ResumesController(createUseCaseMock, getUseCaseMock, parseUseCaseMock, getByIdUseCaseMock);

        // Act
        var result = await controller.Create(requestDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        
        createdResult.Value.Should().BeEquivalentTo(new 
        {
            expectedResume.Id,
            expectedResume.Name,
            expectedResume.Email,
            expectedResume.Phone,
            expectedResume.AreaOfInterest,
            expectedResume.ProfessionalSummary
        });
    }

    [Fact]
    public async Task Get_ShouldReturnOkWithListOfResumes()
    {
        // Arrange
        var createUseCaseMock = Substitute.For<ICreateResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();
        var parseUseCaseMock = Substitute.For<IParseResumeUseCase>();
        var getByIdUseCaseMock = Substitute.For<IGetResumeByIdUseCase>(); 

        var mockResumes = new List<ResumeDto>
        {
            new ResumeDto(Guid.NewGuid(), "João Silva", "joao@email.com", "999999999"),
            new ResumeDto(Guid.NewGuid(), "Maria Santos", "maria@email.com", "888888888")
        };

        getUseCaseMock.ExecuteAsync().Returns(mockResumes);

        var controller = new ResumesController(createUseCaseMock, getUseCaseMock, parseUseCaseMock, getByIdUseCaseMock);

        // Act
        var result = await controller.Get();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResumes = okResult.Value.Should().BeAssignableTo<IEnumerable<ResumeDto>>().Subject;

        returnedResumes.Should().HaveCount(2);
        await getUseCaseMock.Received(1).ExecuteAsync();
    }

    [Fact]
    public async Task Parse_ShouldReturnOkWithParsedData_WhenFileIsValid()
    {
        // Arrange
        var createUseCaseMock = Substitute.For<ICreateResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();
        var parseUseCaseMock = Substitute.For<IParseResumeUseCase>();
        var getByIdUseCaseMock = Substitute.For<IGetResumeByIdUseCase>(); 

        var fakeBytes = new byte[] { 1, 2, 3 };
        var expectedDto = new ParsedResumeDto { Name = "João Silva", Email = "joao@teste.com" };

        parseUseCaseMock.Execute(Arg.Any<byte[]>()).Returns(expectedDto);

        var mockFile = Substitute.For<IFormFile>();
        mockFile.Length.Returns(1024);
        mockFile.FileName.Returns("curriculo.pdf");
        mockFile.ContentType.Returns("application/pdf");
        
        var ms = new MemoryStream(fakeBytes);
        mockFile.OpenReadStream().Returns(ms);

        var controller = new ResumesController(createUseCaseMock, getUseCaseMock, parseUseCaseMock, getByIdUseCaseMock);

        // Act
        var result = await controller.Parse(mockFile);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithResume_WhenResumeExists()
    {
        // Arrange
        var createUseCaseMock = Substitute.For<ICreateResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();
        var parseUseCaseMock = Substitute.For<IParseResumeUseCase>();
        var getByIdUseCaseMock = Substitute.For<IGetResumeByIdUseCase>(); 

        var targetId = Guid.NewGuid();
        var expectedResume = new Resume("Mihaell Alves", "email@teste.com", "11999999999", "Engenharia", "Resumo", null);
        
        getByIdUseCaseMock.ExecuteAsync(targetId).Returns(expectedResume);

        var controller = new ResumesController(createUseCaseMock, getUseCaseMock, parseUseCaseMock, getByIdUseCaseMock);

        // Act
        var result = await controller.GetById(targetId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        
        // Comparar com um objeto anónimo mapeado, igual ao Controller
        okResult.Value.Should().BeEquivalentTo(new 
        {
            expectedResume.Id,
            expectedResume.Name,
            expectedResume.Email,
            expectedResume.Phone,
            expectedResume.AreaOfInterest,
            expectedResume.ProfessionalSummary
        }); 
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenResumeDoesNotExist()
    {
        // Arrange
        var createUseCaseMock = Substitute.For<ICreateResumeUseCase>();
        var getUseCaseMock = Substitute.For<IGetResumesUseCase>();
        var parseUseCaseMock = Substitute.For<IParseResumeUseCase>();
        var getByIdUseCaseMock = Substitute.For<IGetResumeByIdUseCase>();

        var targetId = Guid.NewGuid();
        getByIdUseCaseMock.ExecuteAsync(targetId).Returns((Resume?)null); // Retorna nulo

        var controller = new ResumesController(createUseCaseMock, getUseCaseMock, parseUseCaseMock, getByIdUseCaseMock);

        // Act
        var result = await controller.GetById(targetId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}