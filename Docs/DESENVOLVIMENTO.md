# Registro de Desenvolvimento e Uso de IA

**Como você organizou e executou o trabalho:**
O projeto foi estruturado através de ciclos iterativos ágeis, utilizando a metodologia *Test-Driven Development* (TDD) pautada no fluxo Red-Green-Refactor. Para acelerar a codificação e garantir decisões arquiteturais sólidas, utilizei uma IA como parceira de *Pair Programming*. O agente foi instruído através de um *prompt* detalhado para atuar como Arquiteto de Software especialista em C#/.NET, React e SQL Server, garantindo que o código gerado fosse modular, testável e sem *overengineering*. Em vez de solicitar a aplicação inteira de uma vez, o trabalho foi dividido em blocos lógicos (ex: Modelagem de Domínio, Caso de Uso de Upload, Integração com EF Core, Componentização React), o que permitiu um controle de qualidade rigoroso a cada *commit*. _**Todos resumos de blocos de desenvolvimento podem ser vistos no [LogDevRaw.md](LogDevRaw.md)**_

**As principais decisões técnicas e seus motivos:**
A principal decisão técnica foi a adoção estrita do TDD. Escrever os testes antes da implementação (com `xUnit` e `Vitest`) forçou a criação de um código altamente coeso e com baixo acoplamento. Isso facilitou imensamente a integração, pois conseguimos antecipar erros (como validação de nulos ou formatação achatada de texto do PDF) e reduzir o tempo de *debug*. A arquitetura foi baseada numa *Clean Architecture* simplificada, isolando as regras de negócio (Domínio) da infraestrutura (Entity Framework Core e Kestrel), o que garante alta escalabilidade e manutenibilidade do sistema.

**Quais ferramentas de IA e modelos utilizou, se houver:**
Utilizei o modelo Google Gemini configurado com um agente personalizado (instruído com regras estritas de arquitetura limpa, TDD e foco em código executável). [Instruçoes do Agent](GemInstructionsAI.md)

**Em quais etapas a IA ajudou, com alguns exemplos de pedidos e como as respostas foram aproveitadas:**
A IA apoiou ativamente a construção ponta-a-ponta _(para registro do desenvolvimento completo acesse [LogDevRaw.md](LogDevRaw.md))_, alinhando-se perfeitamente com os requisitos de desenvolvimento Full Stack, integração de sistemas e aplicação de tecnologias emergentes:

* **Desenvolvimento Back End e Front End:** Ajudou a desenhar as APIs RESTful em .NET 8 e a estruturar interfaces responsivas e intuitivas em React utilizando validações robustas com o Zod.


* **Soluções baseadas em IA / Lógica Complexa:** Na etapa de extração de dados do PDF, a IA sugeriu e refinou a implementação de expressões regulares (Regex) e algoritmos heurísticos para isolar o Nome, E-mail e Telefone do candidato, mitigando problemas de achatamento de texto (`\n`) gerados pelo parser.


* **Resolução de Falhas e Desempenho:** Apoiou na identificação e correção de conflitos de dependência no ecossistema .NET (como o `TypeLoadException` do Swagger ao alinhar o pacote `Microsoft.OpenApi`) e auxiliou na construção do `GlobalExceptionHandler` para devolver erros amigáveis padronizados.


* **Documentação e Agilidade:** Após cada funcionalidade concluída, a IA documentou a decisão técnica, ferramentas utilizadas e limitações, mantendo a documentação técnica atualizada em sintonia com os ciclos de entrega.



**O que você precisou corrigir, adaptar ou descartar:**
Inicialmente, a arquitetura foi desenhada para realizar o upload do arquivo PDF e armazená-lo na base de dados (em `byte[]`) juntamente com o cadastro. Contudo, adaptamos essa funcionalidade para utilizar o PDF **apenas como fonte de extração inteligente de dados (Autofill)**, descartando o salvamento físico do arquivo. Essa adaptação manteve o projeto perfeitamente alinhado com o que foi pedido, garantindo um banco de dados leve e um sistema eficiente, deixando o armazenamento de arquivos como uma *feature* mapeada para o *roadmap* futuro. Além disso, a conteinerização completa da aplicação com Docker Compose chegou a ser estruturada, mas foi descartada no final para garantir que a avaliação do teste técnico pudesse ser executada da forma mais simples e nativa possível (`dotnet run` e `npm run dev`).

**Como verificou se a solução estava correta:**
A verificação foi feita em múltiplas camadas para garantir qualidade e segurança:

* **Testes Automatizados Isolados:** Execução contínua da suíte de testes de unidade e integração no Backend (`dotnet test` com xUnit, NSubstitute e FluentAssertions) e no Frontend (`npm run test` com Vitest e React Testing Library simulando o Axios).


* **Testes Manuais E2E:** Navegação completa na aplicação React pelo *browser*, submetendo PDFs reais para validação do autofill e posterior submissão do formulário.


* **Teste de API via Swagger:** Validação direta dos endpoints (`POST /api/resumes/parse`, `POST /api/resumes`, `GET /api/resumes`) utilizando a interface do Swagger UI.



**O tempo aproximado dedicado ao desafio:**

* **Backend:** 5h em desenvolvimento, 2h de integração e bug fix.
* **Frontend:** 1h em desenvolvimento, 1h de integração e bug fix.
* **Testes de features descartadas e documentação:** 1h dedicada à estruturação do Docker Compose e elaboração dos registros técnicos.

**As dificuldades, limitações e melhorias que faria com mais tempo:**

* **Limitações Atuais:** O sistema foca no essencial pedido (Cadastro, Listagem e Detalhes), carecendo dos métodos de Atualização (Update) e Exclusão (Delete) para compor um CRUD completo.
* **Melhorias Funcionais:** Implementaria o upload definitivo do PDF (integrando com um serviço de *Cloud Storage* como AWS S3, exemplo), permitindo que recrutadores fizessem o download do currículo original além de consultar os dados estruturados via formulário.
* **Melhorias de Infraestrutura:** Retomaria e poliria a conteinerização (Docker Compose) com resiliência total (`Retry Patterns` e `Healthchecks`), permitindo subir a aplicação inteira (Banco, API e SPA) com um único comando, algo que priorizei descartar nesta versão para focar estritamente nos requisitos de código avaliados.