# Registro de Desenvolvimento e Decisões de Arquitetura

## Bloco 1: Setup de Infraestrutura Local e Banco de Dados
- **Etapa/Funcionalidade:** Configuração do ambiente de desenvolvimento e banco de dados local.
- **Decisão Técnica e Motivação:** Utilização do Docker e `docker-compose` com a imagem oficial do SQL Server 2022 for Linux. Escolhida para garantir paridade de ambiente entre desenvolvedores, evitar poluição do sistema operacional host e facilitar o reset do banco de dados (destruindo o volume) caso necessário durante os testes.
- **Participação da IA:** O assistente de IA gerou o arquivo `docker-compose.yml` base, incluindo mapeamento de volumes para persistência e configuração de variáveis de ambiente obrigatórias (EULA e SA_PASSWORD).
- **Adaptações e Correções:** Foi adicionado um `healthcheck` nativo usando `sqlcmd` com a flag `-No` (Trust Server Certificate) para contornar erros de certificado SSL no ambiente de desenvolvimento local do SQL Server 2022.
- **Verificação:** Executado `docker-compose up -d`. Container foi inspecionado e verificado como saudável (`healthy`). Conexão validada com sucesso através de execução remota de instrução T-SQL (`SELECT @@VERSION`) via `docker exec`.
- **Limitações Conhecidas:** A senha do banco está "hardcoded" no arquivo `docker-compose.yml`. Embora aceitável para ambiente estrito de desenvolvimento local, esse arquivo nunca deve ser promovido para homologação ou produção sem a injeção adequada de variáveis de ambiente secretas.

## Bloco 2: Troubleshooting de Infraestrutura Local
- **Etapa/Funcionalidade:** Resolução de falha na inicialização do container de banco de dados.
- **Decisão Técnica e Motivação:** Remoção do atributo `version` do `docker-compose.yml`, que tornou-se obsoleto no Docker Compose V2, simplificando o arquivo. Abordagem de isolamento do download da imagem (`docker pull` manual) para bypassar instabilidades na engine de rede do Docker Desktop.
- **Participação da IA:** O assistente orientou a remoção da tag `version` e explicou a natureza de conectividade (DNS/Rede) do erro `EOF` com o registry da Microsoft, sugerindo um fluxo de resolução.
- **Adaptações e Correções:** O manifesto YAML foi limpo. Foram aplicados passos de reinicialização do Docker/rede para concluir o pull da imagem de ~1.5GB do SQL Server.
- **Verificação:** Executado `docker pull mcr.microsoft.com/mssql/server:2022-latest` seguido de `docker-compose up -d`.
- **Limitações Conhecidas:** A engine de rede do Docker no host do desenvolvedor pode sofrer interferência de VPNs, sendo necessário o download das imagens base em rede aberta antes de levantar os serviços.

## Bloco 3: Correção de Timeout do Docker e Inicialização da Solução (TDD - Red)
- **Etapa/Funcionalidade:** Ajuste de rede do Docker Engine, criação da Solution .NET e escrita do primeiro teste unitário.
- **Decisão Técnica e Motivação:** Configurado `"max-concurrent-downloads": 1` no Docker daemon para contornar problemas de `EOF` no download de imagens pesadas (SQL Server). Inicializada uma estrutura minimalista baseada em Vertical Slices/Domain-Driven para o backend usando `webapi` e `xunit`.
- **Participação da IA:** A IA confirmou que a tag da imagem correspondia às versões Ubuntu-based vistas no repositório e forneceu o comando de infraestrutura para limitar a concorrência do Docker. Sugeriu a estrutura inicial do .NET e o primeiro teste unitário validando a criação da entidade `Resume`.
- **Adaptações e Correções:** O foco inicial de testes foi direcionado puramente para validações de domínio (Domain Model) em vez de focar imediatamente em banco de dados ou controllers, respeitando a abordagem Clean/Domain-centric.
- **Verificação:** Executado `dotnet test`. O teste falhou por ausência da classe `Resume`, cumprindo a etapa "Red" do TDD.
- **Limitações Conhecidas:** A estrutura atual mistura a entidade de Domínio diretamente no projeto da API (`ResumeCrud.API.Domain.Entities`). Para um CRUD simples, manteremos tudo em um único projeto de produção para evitar overengineering com dezenas de class libraries.

## Bloco 4: TDD (Fase Green) e Downgrade para .NET 8 (LTS)
- **Etapa/Funcionalidade:** Implementação da entidade de domínio `Resume`, adequação de SDK e limpeza de warnings.
- **Decisão Técnica e Motivação:** A entidade foi criada com setters privados (Encapsulamento) para garantir que as invariâncias (ex: nome obrigatório) sejam respeitadas no construtor. Foi decidido fazer o downgrade do projeto de `.NET 10 Preview` para `.NET 8 LTS`. Versões Preview trazem instabilidades de dependências transitivas (como a vulnerabilidade `NU1903` no gerador do OpenAPI e erros de compilação `CS0200` ao tentar atualizá-lo). O .NET 8 garante um ambiente estável e seguro. Além disso, o FluentAssertions foi fixado na versão `6.12.1` (Apache 2.0) para evitar poluição no console com avisos de licença comercial introduzidos na v7.
- **Participação da IA:** A IA propôs o downgrade para LTS validando a sugestão do desenvolvedor e forneceu os comandos para limpar os avisos do xUnit (Nullable reference types) e do FluentAssertions.
- **Adaptações e Correções:** O parâmetro do teste de unidade foi ajustado para `string?` para respeitar o strict null-checking do C#.
- **Verificação:** Executado `dotnet clean` e `dotnet test`. Testes passando (Green) e console 100% limpo de avisos (Zero Warnings).
- **Limitações Conhecidas:** A validação lança `ArgumentException`, o que exigirá um Middleware global no futuro para traduzir exceções de domínio em HTTP 400.

## Bloco 5: Adequação de API para .NET 8 e Validação de PDF (Fase Green)
- **Etapa/Funcionalidade:** Substituição do gerador OpenAPI nativo do .NET 10 pelo Swashbuckle (.NET 8) e implementação de regras de domínio para arquivos PDF.
- **Decisão Técnica e Motivação:** O template migrado do .NET 10 continha chamadas de extensão (`AddOpenApi`/`MapOpenApi`) inexistentes nativamente no .NET 8. A adoção do pacote `Swashbuckle.AspNetCore` restaurou a interface do Swagger UI sob o padrão LTS. No domínio, constantes foram criadas para definir o tamanho máximo do arquivo (5MB) para evitar alocações excessivas na memória durante requisições abusivas.
- **Participação da IA:** Identificou o conflito de métodos no `Program.cs` via análise de logs/imagem, forneceu a configuração correta do pipeline HTTP para o .NET 8 e implementou as validações de `Length` no array de bytes do PDF.
- **Adaptações e Correções:** O arquivo `Program.cs` foi limpo, removendo o source generator do .NET 10 e mantendo apenas o essencial para inicialização do Swagger.
- **Verificação:** Executado `dotnet test`. Total de 4 testes aprovados (Name validation, Empty PDF validation, Max Size PDF validation).
- **Limitações Conhecidas:** A validação confia apenas no array de bytes (tamanho e nulidade). A verificação da "assinatura real" do arquivo (Magic Numbers para garantir que é de fato um PDF e não um arquivo renomeado) ainda será necessária na camada de Application/Upload.

## Bloco 6: Extração de Dados via Regex e Setup de Controle de Versão (TDD)
- **Etapa/Funcionalidade:** Inicialização do Git e criação do `ResumeParserService` para extrair E-mail e Telefone a partir de textos brutos.
- **Decisão Técnica e Motivação:** A responsabilidade de extrair dados foi separada da leitura binária do PDF. O `ResumeParserService` recebe apenas texto (string), facilitando testes unitários rápidos sem depender de I/O de arquivos. Utilização de Expressões Regulares (Regex) nativas do .NET e `record` para transporte imutável dos dados extraídos. O Git foi inicializado para garantir rastreabilidade das mudanças estruturais.
- **Participação da IA:** Sugeriu os padrões de Regex adaptados para formatação de e-mails universais e telefones no padrão brasileiro (com ou sem DDD, com ou sem o nono dígito). Forneceu os comandos de controle de versão solicitados pelo desenvolvedor.
- **Adaptações e Correções:** O setup do repositório Git e `.gitignore` foi antecipado a pedido do desenvolvedor antes da consolidação do código de parsing para garantir um ponto de restauração seguro.
- **Verificação:** Executado `dotnet test`. O parser extraiu os dados corretamente quando presentes e retornou campos nulos de forma segura em textos sem informações de contato.
- **Limitações Conhecidas:** Expressões regulares são sensíveis a layouts muito exóticos. O parser atual não lida com OCR (imagens escaneadas dentro do PDF), operando sob a premissa de que o PDF possui uma camada de texto digital legível.

## Bloco 7: Extração de Texto de PDF (Fase Green)
- **Etapa/Funcionalidade:** Implementação da leitura de arquivos PDF utilizando a biblioteca `PdfPig`.
- **Decisão Técnica e Motivação:** A escolha do `PdfPig` (UglyToad.PdfPig) deve-se ao fato de ser uma biblioteca leve, 100% gerenciada em C# e de código aberto (sem restrições severas de licenciamento comercial como o iTextSharp/iText7). A extração foca na camada de texto digital embutida no PDF, iterando página a página com o `StringBuilder` para otimizar o uso de memória.
- **Participação da IA:** Forneceu a implementação do serviço encapsulando o `PdfDocument.Open`, que automaticamente valida a integridade do arquivo.
- **Adaptações e Correções:** O tratamento de erro de formato não precisou de verificações manuais de Magic Numbers (ex: `%PDF-1.`), pois delegou-se essa responsabilidade diretamente para a exceção `PdfDocumentFormatException` nativa da biblioteca.
- **Verificação:** Executado `dotnet test`. O teste `ExtractText_ShouldThrowException_WhenByteArrayIsNotAValidPdf` passou com sucesso (Fase Green).
- **Limitações Conhecidas:** A biblioteca `PdfPig` não realiza OCR. PDFs gerados puramente a partir de imagens escaneadas (sem camada de texto invisível) retornarão strings vazias.

## Bloco 8: Orquestração do Upload de Currículo (Fase Green) e Refatoração de Domínio
- **Etapa/Funcionalidade:** Implementação do `UploadResumeUseCase` interligando extração de texto, parsing de Regex e criação da entidade de domínio, além da correção de propriedades omitidas.
- **Decisão Técnica e Motivação:** A lógica de orquestração foi isolada num Use Case (Application Service). Isto permite validar o fluxo completo de negócio em testes unitários utilizando mocks (`NSubstitute`) para as dependências de I/O (leitura do PDF real), garantindo execução rápida e fiável.
- **Participação da IA:** Forneceu a estrutura inicial do caso de uso e corrigiu a omissão do atributo `Phone` no construtor da entidade `Resume`, adaptando todas as camadas afetadas pela quebra de contrato.
- **Adaptações e Correções:** O desenvolvedor identificou precocemente a ausência do campo de telefone durante a execução do TDD (falha de asserção por nulidade). A classe `Resume` e os testes de unidade de domínio associados foram atualizados para suportar o novo parâmetro.
- **Verificação:** Executado `dotnet test`. O mock do `IPdfTextExtractor` devolveu o texto simulado com sucesso, o `ResumeParserService` extraiu o telefone corretamente, e a entidade de domínio foi instanciada com todos os dados preenchidos, tornando a suite 100% verde.
- **Limitações Conhecidas:** O Use Case atual cria e devolve a entidade, mas ainda carece da persistência final (gravação na base de dados via Entity Framework Core).

## Bloco 9: Integração de Persistência no Use Case (TDD - Fase Green)
- **Etapa/Funcionalidade:** Evolução do `UploadResumeUseCase` para execução assíncrona (`async/await`) e injeção da dependência `IResumeRepository`.
- **Decisão Técnica e Motivação:** A operação de gravação numa base de dados deve ser assíncrona para não bloquear a thread principal do Kestrel (servidor web). A utilização da interface `IResumeRepository` aplica o princípio de Inversão de Dependência (SOLID), desacoplando a lógica de negócio do Entity Framework Core que será introduzido posteriormente.
- **Participação da IA:** Propôs a atualização do teste para verificar a chamada ao método `AddAsync` do repositório utilizando `NSubstitute` (Fase Red) e forneceu a implementação da interface e a refatoração assíncrona do Use Case (Fase Green).
- **Adaptações e Correções:** O contrato do Use Case foi alterado de `Execute` para `ExecuteAsync` para refletir a natureza I/O-bound da operação de gravação.
- **Verificação:** Executado `dotnet test`. O `NSubstitute` validou que o método `AddAsync` foi chamado exatamente uma vez (`Received(1)`) com o ID correto da entidade gerada.
- **Limitações Conhecidas:** A interface `IResumeRepository` existe apenas em memória através do mock nos testes. A implementação concreta que dialoga com o SQL Server será criada no próximo passo.

## Bloco 10: Configuração do Entity Framework Core e Migrations
- **Etapa/Funcionalidade:** Mapeamento da entidade `Resume` (Fluent API), configuração do `ResumeDbContext`, implementação concreta do `IResumeRepository` e geração da base de dados SQL Server via Migrations.
- **Decisão Técnica e Motivação:** A utilização do EF Core com Fluent API mantém as regras de mapeamento do banco isoladas na camada de Infraestrutura, sem poluir a entidade de Domínio com atributos (Data Annotations). O repositório encapsula o `DbContext`, preservando a Inversão de Dependência estipulada no Use Case.
- **Participação da IA:** Forneceu a estrutura do `DbContext`, o mapeamento do `byte[]` para varbinary e os comandos da CLI do EF. Identificou e corrigiu o conflito de versões do NuGet ao instalar os pacotes, forçando a trava na versão `8.0.10` para manter compatibilidade com o TargetFramework (LTS).
- **Adaptações e Correções:** O comando padrão de instalação tentou buscar pacotes da versão .NET 10 (Preview), gerando o erro `NU1202`. A correção exigiu o uso da flag `-v 8.0.10` nos comandos `dotnet add package`.
- **Verificação:** Executado `dotnet ef database update`. O banco de dados `ResumeDb` e a tabela `Resumes` foram criados fisicamente no contêiner do SQL Server, respeitando os comprimentos máximos de string (`HasMaxLength`) e restrições de nulidade (`IsRequired`).
- **Limitações Conhecidas:** A connection string está diretamente no `appsettings.Development.json` contendo a senha do banco em plain text, prática aceitável apenas para este contexto de desenvolvimento local.

## Bloco 11: Implementação do Endpoint de Upload (TDD - Fase Green)
- **Etapa/Funcionalidade:** Criação do `ResumesController` para receção de ficheiros via HTTP POST (`multipart/form-data`) e conversão para invocação do Use Case.
- **Decisão Técnica e Motivação:** A receção de ficheiros foi implementada com `IFormFile`. Para manter a pureza da camada de Aplicação (que não deve depender de tipos exclusivos do ASP.NET Core, como `IFormFile`), o Controller assume a responsabilidade de instanciar um `MemoryStream`, extrair o `byte[]` e passá-lo ao Use Case. A resposta utiliza um DTO dinâmico para garantir que o payload binário (`PdfContent`) nunca seja serializado e devolvido no JSON, poupando largura de banda.
- **Participação da IA:** Forneceu o código do `ResumesController` com validações básicas de tipo MIME (`application/pdf`) e presença de ficheiro, orientando também a configuração dos Controllers no `Program.cs`.
- **Adaptações e Correções:** A interface do Use Case foi abstraída (`IUploadResumeUseCase`) na etapa de preparação para permitir a utilização de um mock estrito com `NSubstitute` nos testes da camada de API.
- **Verificação:** Executado `dotnet test`. O teste validou que o endpoint `Upload` processa a simulação do `IFormFile`, invoca o caso de uso corretamente e devolve um `201 Created` sem expor os bytes originais.
- **Limitações Conhecidas:** A validação de MIME type baseada na propriedade `ContentType` do HTTP é frágil e pode ser facilmente falsificada por um cliente mal-intencionado. Para um ambiente de produção rigoroso, a verificação profunda dos "Magic Numbers" do ficheiro manter-se-ia necessária.

## Bloco 12: Tratamento Global de Exceções (TDD - Fase Green)
- **Etapa/Funcionalidade:** Implementação do `GlobalExceptionHandler` utilizando a interface `IExceptionHandler` do .NET 8.
- **Decisão Técnica e Motivação:** Centralizar o tratamento de erros no pipeline evita a proliferação de blocos `try-catch` nos Controllers. A interface `IExceptionHandler` permite mapear de forma limpa exceções específicas de Domínio (ex: `ArgumentException`) para códigos de estado HTTP adequados (400 Bad Request) através do formato padrão `ProblemDetails`.
- **Participação da IA:** Forneceu o teste unitário simulando o contexto HTTP (Fase Red) e a respetiva implementação assíncrona do handler (Fase Green), bem como as instruções de registo no `Program.cs`.
- **Adaptações e Correções:** O uso do `ProblemDetails` foi adotado como padrão de resposta de erro para estar em conformidade com a RFC 7807, facilitando a vida ao frontend que irá consumir a API.
- **Verificação:** Executado `dotnet test`. O `GlobalExceptionHandlerTests` validou que a execução do pipeline alterou o `StatusCode` da resposta para 400 sem lançar a exceção até ao anfitrião do servidor.
- **Limitações Conhecidas:** Neste momento, todas as exceções não previstas respondem com uma mensagem genérica de erro interno. Num ambiente de produção real, faria sentido ligar um logger (`ILogger`) dentro do Handler para registar o rasto (stack trace) destas exceções de nível 500 no Application Insights ou Seq.