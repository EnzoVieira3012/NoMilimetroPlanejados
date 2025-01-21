# Backend - No Milímetro Planejados
Este é o backend de uma aplicação RESTful desenvolvida em .NET 8, seguindo a arquitetura DDD (Domain-Driven Design). A API foi desenvolvida para gerenciar usuários, clientes, comentários e funcionalidades relacionadas, como autenticação JWT, gerenciamento de senhas e segurança de login.

## Índice
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Configuração do Ambiente](#configuração-do-ambiente)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Funcionalidades](#funcionalidades)
- [Endpoints](#endpoints)
- [Segurança](#segurança)
- [Testes](#testes)
- [To-Do](#to-do)
- [Contribuição](#contribuição)

## Tecnologias Utilizadas
- .NET 8
- Entity Framework Core
- PostgreSQL
- JWT (JSON Web Tokens)
- SendGrid (Envio de e-mails)
- Swashbuckle (Swagger)
- Docker (para o banco de dados)
- SonarLint (análise estática de código)

## Pré-requisitos
Antes de começar, você precisará ter as seguintes ferramentas instaladas:

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- [Postman](https://www.postman.com/) ou outra ferramenta para testar APIs
- Uma conta no [SendGrid](https://sendgrid.com/) para envio de e-mails

## Configuração do Ambiente
### Clone o Repositório
git clone https://github.com/EnzoVieira3012/NoMilimetroPlanejados.git
cd backend

### Configure o Banco de Dados no Docker
Certifique-se de que o PostgreSQL esteja configurado no Docker. Use o seguinte `docker-compose.yml`:
version: '3.4'

services:
  db:
    image: postgres:15
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: password
      POSTGRES_DB: backenddb
    volumes:
      - postgres-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  postgres-data:

Inicie o PostgreSQL no Docker: docker-compose up -d

### Configure o Arquivo `appsettings.json`
No diretório backend, configure o arquivo `appsettings.json` com as informações do banco de dados, JWT e SendGrid:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=backenddb;User Id=admin;Password=password;"
  },
  "JwtSettings": {
    "SecretKey": "yA1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6="
  },
  "SendGrid": {
    "ApiKey": "YOUR_SENDGRID_API_KEY",
    "SenderEmail": "enzovieira.trabalho@outlook.com",
    "SenderName": "Sistema de Teste",
    "ReplyToEmail": "enzovieira.pessoal@outlook.com"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Atualize o Banco de Dados
Aplique as migrations para criar as tabelas no banco de dados: dotnet ef database update

## Estrutura do Projeto
A estrutura do projeto foi organizada seguindo a arquitetura DDD (Domain-Driven Design):

Backend/
├── Application/
│   ├── Interfaces/      # Interfaces de serviços e DTOs
│   ├── DTOs/            # Objetos de transferência de dados
│   └── Services/        # Implementação de serviços de aplicação
├── Domain/
│   ├── Entities/        # Entidades do domínio
│   ├── Exceptions/      # Exceções personalizadas
│   └── Interfaces/      # Interfaces dos repositórios
├── Infrastructure/
│   ├── Data/            # Configuração do banco de dados e DbContext
│   ├── Services/        # Implementação dos repositórios
│   └── Migrations/      # Migrations do Entity Framework Core
├── Controllers/         # Controladores da API
├── Program.cs           # Configuração da aplicação
└── Properties/          # Configurações do ambiente de execução

## Funcionalidades
### Usuários
- Registro de usuários.
- Autenticação com JWT.
- Redefinição de senha com envio de código por e-mail (SendGrid).
- Atualização de informações do usuário.
- Exclusão de conta com confirmação de senha.

### Clientes
- Adicionar, listar, editar e deletar clientes.
- Associar e gerenciar comentários para cada cliente.

### Segurança
- JWT com validade configurável (24 horas).
- Bloqueio de IP após 5 tentativas de login falhas (15 minutos de bloqueio).

### Documentação
- Swagger gerado automaticamente (/swagger).

## Endpoints
### Autenticação
| Método | Endpoint                            | Descrição                       |
|--------|-------------------------------------|-----------------------------------|
| POST   | `/api/Auth/register`                | Registra um novo usuário         |
| POST   | `/api/Auth/login`                   | Faz login e gera um token JWT    |
| GET    | `/api/Auth/info`                    | Retorna informações do usuário  |
| PUT    | `/api/Auth/update`                  | Atualiza nome ou e-mail do usuário |
| POST   | `/api/Auth/password-reset-request`  | Envia código de redefinição de senha |
| POST   | `/api/Auth/password-reset`          | Redefine a senha com base no código |
| DELETE | `/api/Auth/delete-account`          | Exclui a conta do usuário logado |

### Clientes
| Método | Endpoint              | Descrição                            |
|--------|-----------------------|------------------------------------|
| POST   | `/api/Customer`       | Adiciona um cliente               |
| GET    | `/api/Customer`       | Lista todos os clientes           |
| GET    | `/api/Customer/{id}`  | Retorna um cliente por ID (com comentários) |
| PUT    | `/api/Customer/{id}`  | Edita as informações de um cliente |
| DELETE | `/api/Customer/{id}`  | Deleta um cliente                 |

### Comentários
| Método | Endpoint                       | Descrição                |
|--------|--------------------------------|------------------------|
| POST   | `/api/Comment/{customerId}`   | Adiciona um comentário a um cliente |
| PUT    | `/api/Comment/{commentId}`    | Edita um comentário     |
| DELETE | `/api/Comment/{commentId}`    | Deleta um comentário    |

## Segurança
### JWT
Todos os endpoints protegidos exigem um token JWT válido no cabeçalho `Authorization`: Authorization: Bearer <seu_token>

### Bloqueio de IP
Após 5 tentativas de login falhas, o IP será bloqueado por 15 minutos.
Essa funcionalidade é gerenciada pelo serviço `LoginAttemptService` usando `MemoryCache`.

## Testes
### Testar Localmente
Inicie o banco de dados no Docker: docker-compose up -d

Execute a API: dotnet run

Acesse a documentação da API no Swagger: http://localhost:5020/swagger

## Contribuição
1. Faça um fork do projeto.
2. Crie uma branch para sua feature: git checkout -b feature/sua-feature
3. Faça as alterações e commit: git commit -m "feat: adiciona nova funcionalidade"
4. Envie para o repositório remoto: git push origin feature/sua-feature
5. Abra um Pull Request.

