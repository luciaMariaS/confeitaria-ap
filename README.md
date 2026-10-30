# Confeitaria API

**Nicho:** Confeitaria por encomenda · **Aluno:** Lucia Maria (@luciaMariaS)
**API no ar:** https://confeitaria-ap.onrender.com/swagger
**Saúde e versão:** https://confeitaria-ap.onrender.com/health · https://confeitaria-ap.onrender.com/api/versao
**Front-end:** https://confeitaria-front-tn0x.onrender.com

## O que a API faz

API REST de uma confeitaria que trabalha por encomenda. Clientes consultam o cardápio (público), criam conta e fazem encomendas com data de retirada. A administradora cadastra e edita os produtos.

**Regra de negócio (409):** cada data de retirada aceita no máximo 5 encomendas não canceladas. A sexta encomenda para o mesmo dia é recusada com 409 Conflict e uma mensagem clara. Cancelar uma encomenda libera a vaga. O "dia" é contado no horário de Brasília (UTC-3).

## Recursos e endpoints

| Verbo  | Rota                                  | Quem pode                                   | Respostas               |
| ------ | ------------------------------------- | ------------------------------------------- | ----------------------- |
| POST   | /api/auth/registro                    | todos                                       | 201, 400, 409           |
| POST   | /api/auth/login                       | todos                                       | 200, 400, 401           |
| GET    | /api/produtos?busca=&pagina=&tamanho= | todos                                       | 200                     |
| GET    | /api/produtos/{id}                    | todos                                       | 200, 404                |
| POST   | /api/produtos                         | admin                                       | 201, 400, 401, 403      |
| PUT    | /api/produtos/{id}                    | admin                                       | 200, 400, 401, 403, 404 |
| DELETE | /api/produtos/{id}                    | admin                                       | 204, 401, 403, 404, 409 |
| POST   | /api/encomendas                       | logado                                      | 201, 400, 401, 404, 409 |
| GET    | /api/encomendas?pagina=&tamanho=      | logado (cliente vê as suas; admin vê todas) | 200, 401                |
| POST   | /api/encomendas/{id}/cancelar         | dono ou admin                               | 200, 401, 403, 404, 409 |
| GET    | /health, /api/versao                  | todos                                       | 200                     |

## Rotas para o corretor testar (sem token)

- Listagem paginada: /api/produtos?pagina=1&tamanho=5
- Busca (exemplo que encontra algo): /api/produtos?busca=bolo
- Busca sem resultado (200, lista vazia): /api/produtos?busca=zzzz
- Detalhe: /api/produtos/1 · inexistente (404): /api/produtos/9999
- Protegida (401 sem token): /api/encomendas

## Contas de teste (fictícias)

- admin: admin@exemplo.com / Admin@2026
- cliente A: ana@exemplo.com / Cliente@2026 · cliente B: bruno@exemplo.com / Cliente@2026

## Banco de dados

PostgreSQL no Neon, com migrations versionadas em `Confeitaria.Api/Migrations`.

- **Usuario:** Id, Nome, Email (único), SenhaHash (BCrypt), Papel (admin ou cliente)
- **Produto:** Id, Nome, Descricao, Preco, Categoria, Ativo
- **Encomenda:** Id, ProdutoId, UsuarioId (dono), Quantidade, DataRetirada (UTC), Observacoes, Status, CriadoEm
- Relação 1 para N: um Produto tem muitas Encomendas; um Usuario tem muitas Encomendas. Excluir é bloqueado (Restrict).
- A regra do 409 está em `Servicos/EncomendaServico.cs` (`CriarAsync`), dentro de uma transação Serializable.

## Como rodar no computador

Variáveis de ambiente necessárias (defina os valores no seu terminal, nunca no repositório):

- `ConnectionStrings__Padrao` (connection string do PostgreSQL no formato Npgsql)
- `Jwt__Segredo` (32 caracteres ou mais)
- `Seed__AdminSenha` (senha da conta admin criada pelo seed)
- `Cors__Origens` (opcional; padrão `http://localhost:5173`)

```
dotnet test
cd Confeitaria.Api
dotnet run
```

As migrations são aplicadas na subida da API (`MigrateAsync`). Para criar uma nova: `dotnet ef migrations add Nome --project Confeitaria.Api`.

## Front-end
Cliente web em React 19 + TypeScript + Vite, na pasta `front/`, com react-router. O visual segue o
design que criei no Figma para o ateliê fictício "Atelier Encanto".

- **Vitrine (pública):** lista os produtos da API com busca e paginação. A busca e a página ficam
  na URL (`?busca=` e `?pagina=`), há esqueletos de carregamento e uma tela "nada encontrado".
- **Login e cadastro:** o login guarda o token no navegador e a sessão sai sozinha quando o token
  expira. Se a API responder 401, a sessão é derrubada com um aviso.
- **Minhas encomendas (logada):** lista as encomendas da pessoa e cria novas, enviando o token no
  cabeçalho `Authorization`. As mensagens de erro da API (400 de validação, 401, 409 de data
  lotada) aparecem na tela.

O endereço da API vem da variável `VITE_API_URL`, definida no Static Site da Render. A API libera
o CORS só para o endereço do front, configurado na variável `Cors__Origens` do serviço da API,
sem `AllowAnyOrigin`.

Para rodar no computador: `cd front`, criar `.env.development` com
`VITE_API_URL=http://localhost:5048`, `npm install` e `npm run dev` (a API precisa estar rodando).

## Como usei a IA

- **IA usada:** Claude (claude.ai)
- **Os 3 pedidos (prompts) que mais ajudaram:**
  1.Colei o enunciado do trabalho, disse que meu nicho era confeitaria por encomenda e que ainda
  não tinha começado. A IA montou o domínio (Produto, Encomenda, Usuário), a regra do 409 e o
  passo a passo. Conferi comparando com o enunciado e com a tabela de nichos. 2. Pedi o serviço de encomendas com a regra do limite por data. A IA fez a contagem dentro de
  uma transação. Conferi no Swagger: a sexta encomenda na mesma data deu 409. 3. Colei os erros do deploy na Render (senha do banco recusada e, depois, o timeout do
  health check) e perguntei o que tinha acontecido. A IA apontou a causa e a correção.
  Conferi o log e o /health no ar.
- \*- **Um erro da IA que percebi e como corrigi (arquivo, trecho e o que mudou):**
  No `Confeitaria.Api/Program.cs`, ao trocar o objeto anônimo do `/api/versao` por um tipo com
  nome (`VersaoDto`, para o Swagger não mostrar um nome feio), as duas linhas
  `app.MapGet("/", () => Results.Redirect("/swagger"))` e `app.MapHealthChecks("/health")`
  sumiram do arquivo. 
  
  O CI ficou verde, porque os meus testes cobrem o domínio e os serviços e
  não as rotas. Quem barrou o problema foi a Render: o deploy falhou com "Timed out after
  waiting for internal health check" e a versão anterior continuou no ar.
  
   Percebi rodando a API no meu computador, onde `/` e `/health` davam 404, e identifiquei que faltavam essas duas linhas. Devolvi as linhas logo depois do `UseSwaggerUI()` e testei de novo
  (`/health` respondeu Healthy). Commit: `fix: restaura as rotas raiz e /health`.

  O que aprendi com isso: o health check da hospedagem é uma proteção a mais, porque ele
  impede que uma versão quebrada entre no ar, e vale rodar a API localmente antes do push.
  
- **O prompt usado para gerar o front:** 
 Mandei prints das telas do meu design no Figma (login,
  login administrativo e catálogo) e a paleta de cores, e pedi: "me dá os códigos logo, bem
  profissional para uma confeitaria e que faça o que o professor pediu". A IA gerou a vitrine, o
  login e a página de encomendas seguindo o design. Depois pedi o cadastro e ajustei o visual
  do login por conta própria.
- **O que aprendi:** Professor do fundo do coração eu tive dificuldade de entender bastante coisa, mas teve algumas coisa que eu aprendi: 400 é dado inválido, 404 é algo que não existe e 409 é um pedido válido que o  estado do sistema não permite, como a data lotada. 401 é "não sei quem você é" e 403 é "sei, mas você não pode". O dono da encomenda vem do token e nunca do corpo da requisição, senão qualquer
  cliente poderia agir em nome de outro. A contagem da regra do 409 fica numa transação para duas
  requisições ao mesmo tempo não passarem do limite. E o health check da hospedagem barra uma
  versão quebrada, mesmo quando o CI fica verde. 
