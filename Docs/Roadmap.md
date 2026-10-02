A principal mudança é que o envio do PDF deixou de ser uma ação de gravação direta na base de dados. Agora, o PDF atua como um "facilitador" (autofill). O fluxo passa a ser:

Utilizador submete o PDF no frontend.

Backend lê, extrai os dados e devolve-os (sem gravar na base de dados).

Frontend recebe os dados, preenche os campos visíveis (Nome, E-mail, Telefone).

Utilizador revê, corrige, preenche os novos campos (Área de Interesse, Resumo Profissional) e submete o formulário completo.

Backend valida as regras de negócio e finalmente grava o registo (com ou sem ficheiro PDF anexado).

Tendo isto em conta, ajustei o nosso planeamento.

🗺️ Novo Roadmap Atualizado
Épico 1: Reestruturação do Domínio (Atual): Adicionar "Área de Interesse" e "Resumo Profissional" à entidade. Tornar o E-mail obrigatório.

Épico 2: Separação de Responsabilidades na API: Criar o endpoint de Extração (/api/resumes/parse - apenas lê o PDF) e ajustar o endpoint de Criação (POST /api/resumes - grava os dados do formulário).

Épico 3: Refatoração do Frontend: O ResumeForm passará a ter todos os campos visíveis e a lógica de autofill ao fazer upload do PDF.

Épico 4: Detalhes do Currículo: Nova página para visualizar o registo completo após clicar na listagem.