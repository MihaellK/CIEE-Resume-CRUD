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
- SQL Server (pode ser LocalDB, Developer Edition ou via Docker)

---

## 🚀 Como Configurar e Executar

### 1. Configuração do Banco de Dados (Backend)

O projeto utiliza o Entity Framework Core com a abordagem Code-First.

1. Navegue até a pasta do backend:
   ```bash
   cd ResumeCrud.API
   ```

2. Crie um arquivo `appsettings.json` na raiz da pasta `ResumeCrud.API`. Você pode usar o modelo abaixo (sem credenciais reais de produção):
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ResumeCrudDb;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
     }
   }
   ```

   *(Nota: Se estiver usando o SQL Server Express/LocalDB no Windows, a connection string pode ser: `"Server=(localdb)\\mssqllocaldb;Database=ResumeCrudDb;Trusted_Connection=True;MultipleActiveResultSets=true"`)*

3. Crie a estrutura do banco de dados rodando as Migrations:
   ```bash
   dotnet ef database update
   ```

4. Execute a API:
   ```bash
   dotnet run
   ```

   A API estará rodando, geralmente em `http://localhost:5092` (verifique o console para a porta exata). A documentação do Swagger ficará acessível em `http://localhost:5092/swagger`.

---

### 2. Configuração do Frontend

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
Na raiz do repositório ou na pasta do backend, execute:
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

* **`DESENVOLVIMENTO.md`**: Um diário de bordo detalhado com todas as decisões arquiteturais, desafios, motivações e uso do ciclo Red-Green-Refactor do TDD.
* **`curriculo_teste.pdf`**: Um arquivo em PDF fictício localizado na raiz deste repositório, que pode ser utilizado para testar a funcionalidade de *Autofill* no formulário.