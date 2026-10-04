# CIEE - Gestão de Currículos (CRUD)

Este projeto é uma aplicação Fullstack (Single Page Application e API RESTful) para o cadastro, listagem e visualização de detalhes de currículos. A aplicação conta com uma funcionalidade de **Autofill Heurístico**, permitindo que os candidatos façam upload de um currículo em PDF para pré-preencher o formulário automaticamente.

O projeto foi construído seguindo os princípios de **Clean Architecture** simplificada e **Test-Driven Development (TDD)**.

---

## 🛠 Tecnologias Utilizadas

**Backend:**
- .NET 8 (C#)
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- PdfPig (Extração de texto de PDF)
- xUnit, NSubstitute, FluentAssertions (Testes Automatizados)

**Frontend:**
- React + TypeScript + Vite
- React Router (Navegação SPA)
- React Hook Form + Zod (Validação de formulários)
- Axios (Cliente HTTP)
- Vitest + React Testing Library (Testes Automatizados)

---

## ⚙️ Pré-requisitos

Para rodar este projeto localmente, você precisará de:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (versão 24.21 ou superior)
- SQL Server (via Docker - testado e recomendado -, LocalDB ou Developer Edition)
- Docker Desktop (Opcional, para facilita gerenciamento de containeres)

---

## 🚀 Como Configurar e Executar

A aplicação já está pré-configurada para rodar rapidamente num ambiente de desenvolvimento local utilizando os arquivos de configuração fornecidos no repositório.

### 1. Configuração do Banco de Dados (SQL Server via Docker)

O projeto inclui um `docker-compose.yml` na raiz que levanta automaticamente uma instância do SQL Server 2022 perfeitamente alinhada com as configurações da API.

1. Na raiz do projeto, execute o comando para iniciar o banco de dados em background:
   ```bash
   docker-compose up -d
   ```

Aguarde alguns segundos. O container inclui um healthcheck nativo que garante que o banco está pronto para receber conexões antes de prosseguirmos.

*(Nota: Se preferir rodar sem Docker, instale o SQL Server localmente e atualize a `DefaultConnection` no arquivo `ResumeCrud.API/appsettings.Development.json` para apontar para a sua instância).*

---

### 2. Configuração da API (Backend)

O projeto utiliza o Entity Framework Core com a abordagem Code-First. As configurações locais já estão prontas no arquivo `appsettings.Development.json`.

1. Navegue até a pasta do backend:
   ```bash
   cd ResumeCrud.API
   ```

2. Crie a estrutura do banco de dados aplicando as Migrations:
   ```bash
   dotnet ef database update
   ```

3. Execute a API:
   ```bash
   dotnet run
   ```

   A API estará rodando, geralmente em `http://localhost:5092` (verifique o console para a porta exata). A documentação do Swagger ficará acessível em `http://localhost:5092/swagger`.

---

### 3. Configuração do Frontend

1. Abra um novo terminal e navegue até a pasta do frontend:
   ```bash
   cd frontend
   ```

2. Instale as dependências:
   ```bash
   npm install
   ```

3. Crie um arquivo `.env.local` na raiz da pasta `frontend` informando a URL da API:
   ```env
   VITE_API_URL=http://localhost:5092/api/resumes
   ```

4. Execute o servidor de desenvolvimento:
   ```bash
   npm run dev
   ```

   O frontend estará acessível no navegador em `http://localhost:5173`.

---

## 🧪 Como Executar os Testes Automatizados

A aplicação possui alta cobertura de testes unitários garantindo regras de negócio, parsers defensivos e estado de componentes UI.

**Testes do Backend (xUnit):**
Na raiz do repositório, execute:
```bash
dotnet test
```

**Testes do Frontend (Vitest):**
Na pasta `frontend`, execute:
```bash
npm run test
```

---

## 📁 Estrutura Adicional

* **`Docs/DESENVOLVIMENTO.md`**: Resumo das decisões técnicas, arquitetura adotada, ferramentas de IA utilizadas e relato sobre o fluxo de desenvolvimento.
* **`Docs/LogDevRaw.md`**: Diário de bordo bruto com o histórico bloco a bloco de todas as implementações, desafios, correções de bugs e o ciclo Red-Green-Refactor do TDD.
* **`Docs/GemInstructionsAI.md`**: Documento que detalha as regras, contexto e comandos de configuração (prompt) do agente personalizado da IA que auxiliou no desenvolvimento no modelo Gemini.
* **`curriculo_teste.pdf`**: Um arquivo em PDF fictício localizado na raiz deste repositório, que pode ser utilizado para testar a funcionalidade de *Autofill* no formulário.