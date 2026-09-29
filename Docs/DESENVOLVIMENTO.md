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