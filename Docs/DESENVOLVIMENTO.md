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

# Inicio do Frontend    
## Bloco 13: Setup Frontend de Testes e Formulário Base (Fase Green)
- **Etapa/Funcionalidade:** Correção da configuração do Vite/Vitest e criação do componente `ResumeForm` básico com validação estrita.
- **Decisão Técnica e Motivação:** A escolha do `react-hook-form` baseia-se na performance (evita re-renders desnecessários típicos de formulários controlados). A integração com o `zod` permite validações declarativas robustas (schema-based) e inferência automática de tipos TypeScript, garantindo paridade com as regras de domínio do backend.
- **Participação da IA:** Identificou o erro tipográfico no `vite.config.ts` através do print enviado pelo desenvolvedor e forneceu o código para sanar a fase Red do TDD do formulário.
- **Adaptações e Correções:** O import `from 'view'` foi corrigido para `from 'vite'`.
- **Verificação:** Executado `npm run test`. O React Testing Library simulou um clique no botão "Enviar" com o campo vazio, e o teste validou com sucesso a renderização condicional da mensagem de erro gerada pelo Zod.
- **Limitações Conhecidas:** O formulário no momento apenas contempla o campo 'nome'. O campo para o arquivo PDF exige um tratamento específico (`FileList`) para extração do `File` via inputs não controlados, o que será abordado na próxima iteração.

## Bloco 13.1: Correção de Tipagem do Vite/Vitest
- **Etapa/Funcionalidade:** Resolução do erro de tipagem TS2769 no arquivo de configuração do Vite.
- **Decisão Técnica e Motivação:** Alterar a importação do `defineConfig` de `vite` para `vitest/config`. Isto garante que o TypeScript reconhece a propriedade `test` injetada pelo ecossistema do Vitest sem causar falhas no servidor de build, mantendo a configuração unificada num único arquivo.
- **Participação da IA:** Identificou a origem do erro de tipagem através da imagem de erro do compilador TS e forneceu o import adequado do pacote Vitest.
- **Adaptações e Correções:** O import `import { defineConfig } from 'vite'` foi substituído por `import { defineConfig } from 'vitest/config'`.
- **Verificação:** Executado `npm run test`. O erro de configuração desapareceu, permitindo que o Vitest executasse a suíte e deixasse o teste do componente `ResumeForm` no estado Green.
- **Limitações Conhecidas:** Nenhuma. A configuração agora está tipada corretamente de forma estrita.

## Bloco 14: Validação de Upload de Arquivo no Frontend (TDD - Fase Green)
- **Etapa/Funcionalidade:** Adição do input de upload de arquivos e validação do `FileList` no esquema do Zod.
- **Decisão Técnica e Motivação:** Como o `<input type="file" />` não pode ser um componente estritamente controlado no React por questões de segurança dos navegadores, a integração com o `react-hook-form` é feita via `register`, o que devolve um objeto `FileList`. Utilizamos `z.any().refine()` no Zod para interceptar este objeto e validar se a sua propriedade `length` é maior que 0. A restrição visual foi reforçada com o atributo HTML nativo `accept=".pdf"`.
- **Participação da IA:** Forneceu a abordagem de validação customizada (`refine`) do Zod para contornar a ausência de um tipo `File/FileList` nativo no esquema padrão do framework.
- **Adaptações e Correções:** O cast `as string` nas mensagens de erro foi adicionado no JSX para acalmar o strict mode do TypeScript que, ao usar `z.any()`, pode não inferir estritamente que a mensagem de retorno será sempre uma string.
- **Verificação:** Executado `npm run test`. O React Testing Library testou a simulação de submissão do formulário preenchendo apenas o nome, e a asserção validou que o componente exibiu corretamente a mensagem "O currículo em PDF é obrigatório".
- **Limitações Conhecidas:** A validação atual verifica apenas a presença do arquivo. As regras adicionais de tamanho (máximo 5MB) e tipo (application/pdf) ainda precisam ser garantidas também no front-end para evitar *roundtrips* desnecessários com o servidor.

## Bloco 15: Validações Estritas de Ficheiro no Frontend (TDD - Fase Green)
- **Etapa/Funcionalidade:** Expansão do schema do Zod para validar o tipo MIME (`application/pdf`) e o tamanho máximo (5MB) do ficheiro submetido.
- **Decisão Técnica e Motivação:** Replicar as regras de domínio do backend no formulário garante um *fail-fast* imediato na interface do utilizador. Isto poupa largura de banda, alivia o servidor de pedidos corrompidos e melhora a experiência do candidato. As regras foram encadeadas com `refine`, utilizando guardas para evitar o lançamento de múltiplos erros conflituantes quando o input está vazio.
- **Participação da IA:** Forneceu a sintaxe encadeada de validações do `FileList` no Zod para fazer a suíte de testes unitários regressar à Fase Green.
- **Adaptações e Correções:** Variáveis constantes (`MAX_FILE_SIZE` e `ACCEPTED_FILE_TYPES`) foram extraídas para o topo do ficheiro, tornando o código mais legível e fácil de manter.
- **Verificação:** Executado `npm run test`. A React Testing Library comprovou a rejeição de ficheiros `.txt` e a recusa imediata de propriedades de tamanho superiores a 5MB, com todas as asserções de texto de erro a passarem.
- **Limitações Conhecidas:** A validação confia no tipo MIME (propriedade `.type`) devolvido pelo navegador. Técnicas mais complexas de *spoofing* ainda precisarão da proteção real do backend (que já existe no nosso caso).

## Bloco 16: Integração HTTP com a API via Axios (TDD - Fase Green)
- **Etapa/Funcionalidade:** Implementação da submissão do formulário utilizando `axios` e `FormData`, incluindo gestão de estado de carregamento e feedback ao utilizador.
- **Decisão Técnica e Motivação:** O envio de ficheiros exige obrigatoriamente o formato `multipart/form-data`. O objeto nativo `FormData` abstrai a montagem binária necessária para o browser. O Vitest permitiu realizar um mock completo do Axios, garantindo que o teste unitário valida a assinatura do método POST sem efetuar chamadas reais de rede, preservando a velocidade da suíte.
- **Participação da IA:** Forneceu a simulação do Axios no contexto de testes (Fase Red) e construiu a lógica do `onSubmit` extraindo o ficheiro do `FileList` para o injetar no payload, além de incorporar estados de UI (`isSubmitting`, `feedback`).
- **Adaptações e Correções:** O endpoint do servidor (`https://localhost:7198/api/resumes/upload`) foi fixado no código temporariamente de modo a garantir a aprovação no teste unitário escrito na etapa anterior. Foi introduzida a limpeza do formulário (`reset()`) em caso de sucesso.
- **Verificação:** Executado `npm run test`. O espião (spy) do Vitest intercetou a chamada `axios.post`, confirmando que ocorreu exatamente 1 vez com o endpoint estipulado, recebendo um objeto instanciado a partir de `FormData` e os cabeçalhos MIME corretos.
- **Limitações Conhecidas:** O URL da API encontra-se *hardcoded* no componente. Num ambiente real, este valor deve provir do ficheiro `.env` através de variáveis globais do Vite (`import.meta.env.VITE_API_URL`). O tratamento de exceções (bloco `catch`) também não discrimina ainda o formato `ProblemDetails` retornado pela nossa API em caso de erro 400.

## Bloco 17: Configuração de CORS e Variáveis de Ambiente
- **Etapa/Funcionalidade:** Habilitação do middleware de CORS no ASP.NET Core e migração do URL da API para o ficheiro `.env.local` no frontend.
- **Decisão Técnica e Motivação:** O CORS restringe a comunicação entre origens diferentes por motivos de segurança. A configuração explícita do `WithOrigins` apontando para a porta do Vite (`5173`) permite o tráfego local seguro durante o desenvolvimento. O uso do `import.meta.env` segue as práticas recomendadas do Vite para injetar configurações específicas de ambiente (Dev/Prod) no tempo de build, evitando chumbamento de URLs no código fonte.
- **Participação da IA:** Forneceu as configurações exatas do pipeline do ASP.NET Core e a sintaxe de variáveis de ambiente do Vite.
- **Adaptações e Correções:** A porta da API no `.env.local` requer ajuste manual caso o Kestrel aloque uma porta diferente da predefinida (7198).
- **Verificação:** Execução de teste manual (E2E) com o backend e frontend a correrem em simultâneo. Submissão de um PDF resultou num código HTTP 201 Created no separador Network das ferramentas de desenvolvimento do navegador e inserção bem-sucedida na base de dados.
- **Limitações Conhecidas:** A política de CORS atual está fixada no `localhost:5173`. Para produção, esta origem terá de ser parametrizada através do `appsettings.json` para suportar o domínio de alojamento real (ex: Vercel, Netlify ou domínio próprio).

## Bloco 18: Montagem do Componente Principal e Limpeza do Template
- **Etapa/Funcionalidade:** Substituição do código padrão do Vite (`App.tsx` e `main.tsx`) pela montagem estrutural do componente `ResumeForm`.
- **Decisão Técnica e Motivação:** Limpeza do *boilerplate* desnecessário para expor o formulário desenvolvido através do TDD. A estrutura foi mantida simples, utilizando o `StrictMode` do React para detetar potenciais efeitos colaterais na renderização.
- **Participação da IA:** Forneceu a refatoração do ponto de entrada da aplicação, eliminando o código não utilizado do template inicial.
- **Adaptações e Correções:** O CSS global gerado pelo Vite pode ser removido para evitar interferências visuais indesejadas. Estilos inline básicos foram aplicados ao contêiner principal para centralizar o formulário no ecrã.
- **Verificação:** Execução de `npm run dev` e validação visual no navegador (`http://localhost:5173`) para confirmar a integração do componente.
- **Limitações Conhecidas:** A aplicação ainda não possui um sistema de rotas (React Router) para navegação entre a página de upload e a futura página de listagem (Grid).

## Bloco 18.1: Correção de Tipagem do Vite Client (import.meta.env)
- **Etapa/Funcionalidade:** Inclusão dos tipos globais do Vite no compilador TypeScript.
- **Decisão Técnica e Motivação:** O TypeScript reporta erro na leitura de variáveis de ambiente via `import.meta.env` por desconhecer a API específica do Vite. A adição de `"vite/client"` no `tsconfig.app.json` (ou via ficheiro de declaração global) instrui o compilador a reconhecer essa assinatura, devolvendo o *IntelliSense* correto sem comprometer o *build*.
- **Participação da IA:** Identificou o erro de linting a partir da captura de ecrã e forneceu a configuração exata do compilador.
- **Adaptações e Correções:** Atualização da secção `"types"` do TSConfig.
- **Verificação:** Validação visual no VS Code indicando o desaparecimento do erro `ts(2339)` e manutenção da compilação verde no terminal.
- **Limitações Conhecidas:** Nenhuma. Prática padrão da ferramenta.

## Bloco 18.2: Resolução de Incompatibilidade de Protocolo (HTTP/HTTPS)
- **Etapa/Funcionalidade:** Correção da variável de ambiente `VITE_API_URL` para coincidir com o protocolo exposto pelo Kestrel.
- **Decisão Técnica e Motivação:** O frontend tentava comunicar via HTTPS com uma porta do backend configurada apenas para HTTP. A correção consistiu em alinhar os protocolos no ficheiro `.env.local`, garantindo que o CORS e a submissão de formulários operem sem interrupções de handshake SSL falhados no ambiente local.
- **Participação da IA:** Analisou as evidências do terminal e do separador Network para identificar a divergência de protocolo (`http://` vs `https://`) e orientou o reinício do bundler (Vite) para recarregar as configurações de ambiente.
- **Adaptações e Correções:** O prefixo do URL no frontend foi alterado de `https` para `http`.
- **Verificação:** Execução manual. Após alinhamento, o envio do `FormData` resultou numa conexão bem-sucedida (Status 201).
- **Limitações Conhecidas:** Num ambiente de produção, tanto o frontend como a API devem obrigatoriamente operar sob HTTPS para garantir a encriptação do tráfego (dados sensíveis do currículo).