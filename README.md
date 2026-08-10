✈️ LisAeroGest
Plataforma de Gestão Aeroportuária de Lisboa
Projeto final desenvolvido no âmbito do Curso CET 105 — Tecnologia e Programação de Sistemas de Informação (Nível 5) no CINEL Lisboa.
---
📋 Descrição
O LisAeroGest é uma plataforma web de gestão operacional inspirada no Aeroporto Humberto Delgado (LIS), que permite gerir de forma integrada voos, companhias aéreas, gates, aeronaves, passageiros, bilhética e check-in.
---
🚀 Funcionalidades
Gestão Operacional (Admin / Funcionário)
🛫 Painel de partidas e chegadas em tempo real
✈️ Gestão de voos com estados operacionais (Previsto, Check-in, A Embarcar, Partiu, Atrasado, Cancelado)
🏢 Gestão de aeroportos com código IATA
🏦 Gestão de companhias aéreas
🚪 Gestão de gates por terminal
✈️ Gestão de aeronaves com capacidade por classe
👥 Gestão de passageiros e utilizadores
📊 Dashboard com estatísticas e gráficos em tempo real
💬 Fórum interno entre funcionários e administradores
🔔 Sistema de notificações
📄 Exportação de voos para PDF e XML
🌤️ Integração com API meteorológica (OpenWeatherMap)
Bilhética (Passageiro)
🔍 Pesquisa de voos disponíveis
🪑 Seleção de lugar por classe (Económica / Executiva)
🛒 Carrinho de compras com extras (bagagem, refeição)
🎫 Emissão de bilhete com QR Code
✅ Check-in online com cartão de embarque em PDF
Autenticação
📧 Registo com confirmação de email
🔐 Login com cookie (web) e JWT (API)
🔑 Recuperação e alteração de password
👤 Três perfis de acesso: Admin, Funcionário, Passageiro
---
🛠️ Tecnologias
Tecnologia	Função
ASP.NET MVC (.NET 8)	Framework principal da aplicação web
Entity Framework Core 8	ORM — Code First com Migrations
SQL Server	Base de dados em desenvolvimento
PostgreSQL (Supabase)	Base de dados em produção
ASP.NET Identity	Autenticação e gestão de utilizadores
JWT	Autenticação para a API REST
Azure Blob Storage	Armazenamento de imagens
QuestPDF	Geração de PDFs (bilhetes e cartões de embarque)
QRCoder	Geração de QR Codes
MailKit	Envio de emails
Chart.js	Gráficos no dashboard
OpenWeatherMap API	Dados meteorológicos
Bootstrap 5	Interface responsiva
Git + GitHub	Controlo de versão
---
🏗️ Arquitetura
```
LisAeroGest/
├── Controllers/          # MVC Controllers + API Controllers
│   └── Api/              # API REST com JWT
├── Data/
│   ├── Entities/         # Entidades da base de dados
│   ├── Interfaces/       # Interfaces dos repositórios
│   └── Repositories/     # Implementação dos repositórios
├── Helpers/              # UserHelper, BlobHelper, MailHelper, ImageHelper
├── Models/               # ViewModels
├── Services/             # PdfService, QrCodeService, WeatherService
├── Migrations/
│   ├── SqlServer/        # Migrations para desenvolvimento
│   └── Postgres/         # Migrations para produção
└── Views/                # Views Razor (.cshtml)
```
Padrões de design aplicados:
MVC (Model-View-Controller)
Repository Pattern com interfaces
Injeção de Dependências
Soft Delete com interceptor do EF Core
Query Filters globais
---
⚙️ Como correr o projeto
Pré-requisitos
.NET 8 SDK
SQL Server (LocalDB ou superior)
Visual Studio 2022
Passos
1. Clonar o repositório:
```bash
git clone https://github.com/Felix-hub123/LisAeroGest.git
cd LisAeroGest
```
2. Configurar o `appsettings.json` (com base no `appsettings.Example.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LisAeroGest;Trusted_Connection=True;"
  },
  "Tokens": {
    "Key": "YOUR_JWT_SECRET_KEY",
    "Issuer": "LisAeroGest",
    "Audience": "LisAeroGestUsers"
  },
  "Mail": {
    "From": "your@email.com",
    "Smtp": "smtp.server.com",
    "Port": 587,
    "Password": "your_password"
  }
}
```
3. Aplicar as migrations:
```bash
Update-Database
```
4. Correr a aplicação:
```bash
dotnet run
```
A base de dados é populada automaticamente com dados de seed na primeira execução.
Credenciais de teste
Role	Email	Password
Admin	admin@lisaerogest.pt	Admin123!
---
👤 Perfis de Utilizador
Role	Permissões
Admin	Acesso total — gestão de tudo, dashboard, fórum
Funcionário	Gestão de voos, check-in presencial, fórum
Passageiro	Compra de bilhetes, check-in online, cartão de embarque
---
📁 Estrutura da Base de Dados
As principais entidades do sistema:
`User` — utilizadores (herda de IdentityUser)
`Passenger` — perfil de passageiro
`Airport` — aeroportos com código IATA
`Airline` — companhias aéreas
`Aircraft` — aeronaves com capacidade por classe
`Seat` — assentos físicos
`Gate` — portões de embarque
`Flight` — voos agendados
`Ticket` — bilhetes comprados
`BoardingPass` — cartões de embarque
`ForumTopic` / `ForumComment` — fórum interno
`Notification` — notificações do sistema
---
🌐 Deploy
A aplicação está disponível em produção no Render, com base de dados PostgreSQL no Supabase.
---
📚 Curso
Instituição: CINEL — Centro de Formação Profissional da Indústria Electrónica, Energia, Telecomunicações e Tecnologias de Informação
Curso: CET 105 — Tecnologia e Programação de Sistemas de Informação (Nível 5)
UFCD: 5425 — Projeto
Formadores: António Pacheco e Susana Pimentel
Ano: 2026
