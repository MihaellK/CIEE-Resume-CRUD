Arquitetura do Backend: ResumeCrud.API
O backend foi estruturado com base numa Clean Architecture simplificada, focada na separação de responsabilidades (Separation of Concerns) e na Inversão de Dependência (SOLID). A aplicação está dividida em quatro camadas lógicas principais, contidas no mesmo projeto para evitar overengineering, mas rigorosamente isoladas por namespaces.

1. Camada de Domínio (Domain)
Representa o núcleo da aplicação. Não tem dependências externas (frameworks web ou bibliotecas de persistência).

Entities/Resume.cs: Entidade de domínio rica. Encapsula os dados do currículo (Id, Nome, Email, Telefone, Bytes do PDF) e garante a sua própria integridade no momento da instanciação (não permite nomes vazios, valida o tamanho máximo de 5MB e garante que os bytes do PDF não estão vazios). Lança ArgumentException em caso de violação.

Repositories/IResumeRepository.cs: Interface que define o contrato de persistência. Garante que o Domínio e a Aplicação não saibam como os dados são guardados, apenas que são guardados.

2. Camada de Aplicação (Application)
Contém os casos de uso e os serviços específicos do negócio, orquestrando o fluxo de trabalho.

Services/IUploadResumeUseCase.cs & UploadResumeUseCase.cs: O orquestrador principal. Recebe os bytes do ficheiro HTTP, aciona o extrator de PDF, passa o texto para o parser de Regex, instancia a entidade Resume e, por fim, chama o repositório para guardar na base de dados.

Services/IPdfTextExtractor.cs & PdfTextExtractorService.cs: Wrapper para a biblioteca PdfPig. Isola a complexidade de ler as páginas binárias do ficheiro PDF e extrai o conteúdo digital em formato de texto bruto (String).

Services/ResumeParserService.cs: Serviço de lógica pura (Regex). Recebe o texto bruto do currículo e utiliza expressões regulares para encontrar padrões de E-mail universal e Telefones no formato brasileiro.

3. Camada de Infraestrutura (Infrastructure)
Responsável pela comunicação com o mundo externo (Base de Dados e Interceção de pipeline HTTP).

Data/ResumeDbContext.cs: O contexto do Entity Framework Core. Utiliza Fluent API para mapear a entidade Resume para a tabela SQL, definindo chaves primárias, tamanhos máximos de colunas e obrigatoriedade de campos sem poluir a entidade com Data Annotations.

Repositories/ResumeRepository.cs: Implementação concreta do IResumeRepository que interage diretamente com o DbContext para persistir dados no SQL Server.

Handlers/GlobalExceptionHandler.cs: Middleware nativo do .NET 8 (IExceptionHandler). Interceta exceções lançadas pelas camadas inferiores (como os ArgumentException do Domínio) e traduz para respostas HTTP 400 (Bad Request) padronizadas em formato ProblemDetails, protegendo a API de devolver erros 500 para falhas de validação.

4. Camada de API (Controllers e Program.cs)
O ponto de entrada da aplicação.

Controllers/ResumesController.cs: Expõe o endpoint POST /api/resumes/upload. Recebe o formulário multipart (dados e IFormFile), converte o ficheiro em byte[] via MemoryStream e delega a execução para o UseCase. Devolve um DTO dinâmico (201 Created) para não expor a carga binária do PDF na resposta JSON.

Program.cs: Configura a Inversão de Dependência (DI Container), inicializa o DbContext com a Connection String, regista o GlobalExceptionHandler e expõe o Swagger UI.

Arquitetura de Testes: ResumeCrud.Tests
Os testes seguem a pirâmide de testes unitários com foco estrito nas regras de negócio e de orquestração, utilizando xUnit, FluentAssertions (para validações mais legíveis) e NSubstitute (para isolamento de dependências).

1. Testes de Domínio (Domain)
ResumeTests.cs: Valida a integridade da entidade principal. Testa se o construtor rejeita nomes nulos/vazios, se impede a criação com arrays de PDF vazios (0 bytes) e se lança erro rigoroso ao tentar injetar ficheiros acima de 5MB.

2. Testes de Aplicação (Application)
ResumeParserServiceTests.cs: Garante que o parser Regex extrai corretamente e-mails e números de telefone complexos, e assegura que devolve null quando o texto do currículo não contém estes dados de contato.

PdfTextExtractorTests.cs: Verifica o comportamento da biblioteca PdfPig face a ficheiros corrompidos (bytes arbitrários que fingem ser PDF), garantindo que a biblioteca rejeita o conteúdo adequadamente.

UploadResumeUseCaseTests.cs: Testa a orquestração. Instancia mocks tanto do Repositório como do Extrator de PDF, garantindo que o Use Case chama todos os serviços na ordem correta, instancia a entidade com os dados parseados e invoca o AddAsync do repositório exatamente uma vez.

3. Testes de API (API)
ResumesControllerTests.cs: Valida o contrato HTTP. Utiliza um mock do Use Case e do IFormFile para garantir que o Controller faz a abstração correta da rede, devolvendo um estado 201 Created e formatando o output JSON de forma segura sem vazar os bytes do documento.

GlobalExceptionHandlerTests.cs: Simula um pipeline do ASP.NET Core fornecendo um DefaultHttpContext. Garante que uma exceção do Domínio injetada resulta na alteração do código de estado de resposta para 400 Bad Request.