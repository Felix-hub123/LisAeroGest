Aplicação Web
A aplicação Web foi desenvolvida com ASP.NET Core MVC e disponibiliza diferentes funcionalidades consoante a role do utilizador.

👨‍💼 Administrador
O Administrador possui acesso global à plataforma.
Principais funcionalidades:
- gestão de aeroportos;
- gestão de companhias aéreas;
- gestão de aeronaves;
- gestão de gates;
- gestão de voos;
- gestão de passageiros;
- gestão de funcionários;
- consulta de bilhetes;
- dashboard administrativo;
- estatísticas e indicadores;
- fórum interno;
- notificações;
- exportação de informação.
  
👨‍🔧 Funcionário
O Funcionário está orientado para as operações aeroportuárias.
Principais funcionalidades:
- consulta e gestão de voos;
- acompanhamento operacional;
- gestão de passageiros;
- check-in;
- operações relacionadas com gates;
- consulta de bilhetes;
- fórum interno;
- notificações.
  
👤 Passageiro
O Passageiro possui uma experiência orientada para a sua viagem.
Principais funcionalidades:
- pesquisar voos;
- consultar detalhes de voo;
- selecionar lugar;
- reservar/comprar bilhete;
- adicionar extras;
- consultar histórico de viagens;
- efetuar check-in;
- consultar informações da viagem;
- gerir o seu perfil;
- receber notificações.
📱 Aplicação Mobile
O LisAeroGest possui também uma aplicação desenvolvida em .NET MAUI.
A aplicação mobile foi concebida com uma finalidade própria e não apenas como uma reprodução da aplicação Web.
Entre as funcionalidades implementadas encontram-se:
- autenticação através da API;
- painel do passageiro;
- consulta de partidas;
- detalhes de voo;
- seleção de lugar;
- processo de pagamento;
- check-in;
- histórico;
- favoritos;
- notificações;
- área operacional destinada a funcionários.
  
A comunicação entre a aplicação mobile e o backend é realizada através da REST API com autenticação JWT.
🔌 REST API
A solução disponibiliza uma API REST responsável pela comunicação com aplicações externas e com a aplicação mobile.
A API utiliza:
- REST;
- JSON;
- JWT Bearer Authentication;
- autorização baseada em roles;
- DTOs;
- serviços;
- repositories;
- Entity Framework Core.
  
Exemplo conceptual:
Mobile App
    │
    │ HTTPS / JSON
    ▼
REST API
    │
    │ JWT
    ▼
Services
    │
    ▼
Repositories
    │
    ▼
Entity Framework Core
    │
    ▼
PostgreSQL / SQL Server

🚀 Principais funcionalidades
✈️ Gestão aeroportuária
- painel de partidas e chegadas;
- criação e gestão de voos;
- estados operacionais dos voos;
- aeroportos com códigos IATA;
- companhias aéreas;
- aeronaves;
- lugares por classe;
- gates;
- terminais.
🎫 Bilhética
- pesquisa de voos;
- seleção de voo;
- seleção de lugar;
- classes Económica e Executiva;
- extras de viagem;
- reservas;
- emissão de bilhetes;
- histórico de viagens.
✅ Check-in
- identificação do passageiro;
- validação do bilhete;
- validação do voo;
- atribuição/confirmação de lugar;
- atualização do estado da viagem.
🔐 Autenticação e autorização
- ASP.NET Core Identity;
- autenticação Web através de cookies;
- autenticação API através de JWT;
- confirmação de email;
- recuperação de password;
- alteração de password;
- autorização baseada em roles.
Roles existentes:
Admin
Funcionario
Passageiro

🔔 Notificações
O sistema permite apresentar notificações relacionadas com informação relevante para os utilizadores.

💬 Fórum
Administradores e funcionários podem utilizar um fórum interno para comunicação relacionada com as operações da plataforma.

🌤️ Meteorologia
Integração com a API OpenWeatherMap para apresentar informação meteorológica relacionada com os aeroportos/voos.

📄 Exportação
A plataforma permite gerar documentos através de:
- PDF;
- XML.
🛠️ Stack Tecnológica
Tecnologia	Utilização
C#	Linguagem principal
.NET 8	Plataforma
ASP.NET Core MVC	Aplicação Web
ASP.NET Core Web API	REST API
.NET MAUI	Aplicação Mobile
Entity Framework Core 8	ORM
SQL Server	Base de dados em desenvolvimento
PostgreSQL	Base de dados em produção
ASP.NET Core Identity	Gestão de utilizadores
JWT	Autenticação da API
Bootstrap 5	Interface Web
QuestPDF	Geração de PDFs
QRCoder	Geração de QR Codes
MailKit	Emails
Chart.js	Gráficos e dashboards
OpenWeatherMap	API meteorológica
Git	Controlo de versão
GitHub	Repositório e colaboração
Docker	Containerização
Render	Hosting
Supabase	PostgreSQL em produção


Algumas integrações e serviços podem variar consoante o ambiente de desenvolvimento ou produção.

🏗️ Arquitetura
A aplicação segue uma arquitetura baseada em separação de responsabilidades.
LisAeroGest/
│
├── Controllers/
│   ├── MVC Controllers
│   └── API Controllers
│
├── Data/
│   ├── Entities/
│   ├── Repositories/
│   └── DataContext
│
├── Helpers/
│
├── Models/
│   ├── ViewModels
│   └── DTOs
│
├── Services/
│
├── Views/
│
├── Migrations/
│
└── wwwroot/

Fluxo principal:
View / Mobile
      │
      ▼
Controller / API Controller
      │
      ▼
Service
      │
      ▼
Repository
      │
      ▼
Entity Framework Core
      │
      ▼
Database

🧠 Princípios e padrões utilizados
O projeto aplica diferentes conceitos de desenvolvimento de software:
- MVC — Model View Controller;
- Repository Pattern;
- Dependency Injection;
- Separation of Concerns;
- Service Layer;
- DTOs;
- ViewModels;
- REST;
- autenticação JWT;
- Role-Based Authorization;
- Soft Delete;
- Global Query Filters;
- Entity Framework Code First;
- migrations;
- programação assíncrona com async/await.
🗄️ Principais entidades
User
Passenger
Airport
Airline
Aircraft
Seat
Gate
Flight
Ticket
BoardingPass
Notification
ForumTopic
ForumComment

Exemplo simplificado das relações:
Airline
   │
   └── Aircraft
          │
          └── Seat

Airport
   │
   ├── Flight Origin
   └── Flight Destination

Flight
   │
   ├── Aircraft
   ├── Gate
   └── Ticket
          │
          ├── Passenger
          └── Seat

⚙️ Executar localmente
Pré-requisitos
- .NET 8 SDK
- Visual Studio 2022
- SQL Server
- Git
1. Clonar
git clone https://github.com/Felix-hub123/LisAeroGest.git
cd LisAeroGest

2. Configuração
Criar/configurar o appsettings.json a partir do ficheiro de exemplo.
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING"
  },

  "Tokens": {
    "Key": "YOUR_JWT_SECRET",
    "Issuer": "LisAeroGest",
    "Audience": "LisAeroGestUsers"
  },

  "Mail": {
    "From": "YOUR_EMAIL",
    "Smtp": "YOUR_SMTP_SERVER",
    "Port": 587,
    "Password": "YOUR_PASSWORD"
  }
}

Nunca guardar passwords, connection strings ou secrets reais no GitHub.

3. Base de dados
Na Package Manager Console:
Update-Database

ou através do .NET CLI:
dotnet ef database update

4. Executar
dotnet run

🐳 Docker
O projeto suporta execução através de Docker.
Exemplo:
docker build -t lisaerogest .
docker run -p 8080:8080 lisaerogest

🌐 Produção
A aplicação foi preparada para execução em ambiente de produção utilizando:
Docker
   ↓
Render
   ↓
ASP.NET Core
   ↓
PostgreSQL
   ↓
Supabase

🔐 Segurança
Foram implementados mecanismos como:
- hashing de passwords através do ASP.NET Identity;
- autenticação JWT;
- autenticação por cookie;
- autorização baseada em roles;
- proteção CSRF;
- confirmação de email;
- recuperação segura de password;
- validação server-side;
- proteção de endpoints;
- configuração de secrets através de variáveis de ambiente.
🧪 Competências demonstradas
O LisAeroGest demonstra experiência prática em:
C#
.NET 8
ASP.NET Core MVC
ASP.NET Core Web API
Entity Framework Core
SQL
PostgreSQL
REST APIs
JWT
Identity
Dependency Injection
Repository Pattern
Service Layer
Docker
Git
GitHub
.NET MAUI
Integração de APIs externas
Deployment
Debugging

📸 Screenshots


🧩 Desafios técnicos
Durante o desenvolvimento foram trabalhados problemas relacionados com:
- autenticação Web e API;
- controlo de acesso por roles;
- arquitetura e separação de responsabilidades;
- comunicação Web ↔ API ↔ Mobile;
- Entity Framework Core;
- migrations;
- diferentes bases de dados entre Development e Production;
- armazenamento e apresentação de imagens;
- deploy através de containers;
- integração de APIs externas;
- tratamento de erros;
- responsividade;
- fluxo de compra/check-in.
📖 Contexto académico
Instituição:
CINEL — Centro de Formação Profissional da Indústria Electrónica, Energia, Telecomunicações e Tecnologias de Informação
Curso:
CET 105 — Tecnologia e Programação de Sistemas de Informação
Nível:
Nível 5
UFCD:
5425 — Projeto
Formadores:
António Pacheco e Susana Pimentel
Ano:
2026
👨‍💻 Autor
Félix Franco Clemente
Projeto desenvolvido como trabalho final do CET 105.
GitHub:
https://github.com/Felix-hub123
📌 Estado
✅ Aplicação Web
✅ REST API
✅ Base de dados
✅ Autenticação e Roles
✅ Deployment
✅ Aplicação Mobile

⭐ Sobre este projeto
O LisAeroGest foi desenvolvido como projeto académico, mas estruturado com o objetivo de aplicar conceitos utilizados em aplicações empresariais reais, desde a arquitetura backend até à integração entre diferentes clientes através de uma REST API.
