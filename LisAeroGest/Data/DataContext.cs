using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;


namespace LisAeroGest.Data
{


    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Gate> Gates { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Tabela única de bilhetes
        /// (Reservados, Pagos, CheckedIn, etc.).
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<ForumTopic> ForumTopics { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<BoardingPass> BoardingPasses { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Histórico das comunicações enviadas
        /// aos passageiros de cada voo.
        /// </summary>
        public DbSet<FlightCommunication> FlightCommunications { get; set; }


        private readonly IHttpContextAccessor? _httpContextAccessor;

        public DataContext(
             DbContextOptions<DataContext> options,
             IHttpContextAccessor httpContextAccessor)
             : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        // Usado pelos contextos específicos de migrations
        // DataContextSqlServer / DataContextPostgres.
        protected DataContext(
            DbContextOptions options)
            : base(options)
        {
            _httpContextAccessor = null;
        }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {

            // Necessário para o Identity funcionar corretamente
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // AUDITORIA
            // =========================================================

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Flight)
                .WithMany()
                .HasForeignKey(a => a.FlightId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Ticket)
                .WithMany()
                .HasForeignKey(a => a.TicketId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            #region Filtros Globais de Soft Delete

            modelBuilder.Entity<Airport>()
                .HasQueryFilter(a => !a.WasDeleted);

            modelBuilder.Entity<Airline>()
                .HasQueryFilter(a => !a.WasDeleted);

            modelBuilder.Entity<Gate>()
                .HasQueryFilter(g => !g.WasDeleted);

            modelBuilder.Entity<Aircraft>()
                .HasQueryFilter(a => !a.WasDeleted);

            modelBuilder.Entity<Seat>()
                .HasQueryFilter(s => !s.WasDeleted);

            modelBuilder.Entity<Flight>()
                .HasQueryFilter(f => !f.WasDeleted);

            modelBuilder.Entity<Passenger>()
                .HasQueryFilter(p => !p.WasDeleted);

            modelBuilder.Entity<Ticket>()
                .HasQueryFilter(t => !t.WasDeleted);

            modelBuilder.Entity<ForumTopic>()
                .HasQueryFilter(f => !f.WasDeleted);

            modelBuilder.Entity<ForumComment>()
                .HasQueryFilter(f => !f.WasDeleted);

            #endregion


            #region Índices Únicos (Prevenção de Duplicados)

            var isSqlServer = Database.IsSqlServer();

            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.Name)
                .IsUnique()
                .HasFilter(
                    isSqlServer
                        ? "[WasDeleted] = 0"
                        : "\"WasDeleted\" = false");

            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATACode)
                .IsUnique()
                .HasFilter(
                    isSqlServer
                        ? "[WasDeleted] = 0"
                        : "\"WasDeleted\" = false");

            modelBuilder.Entity<Airline>()
                .HasIndex(a => a.Name)
                .IsUnique()
                .HasFilter(
                    isSqlServer
                        ? "[WasDeleted] = 0"
                        : "\"WasDeleted\" = false");

            modelBuilder.Entity<Airline>()
                .HasIndex(a => a.IATACode)
                .IsUnique()
                .HasFilter(
                    isSqlServer
                        ? "[WasDeleted] = 0"
                        : "\"WasDeleted\" = false");

            #endregion


            #region Tipos Decimais

            modelBuilder.Entity<Airport>()
                .Property(a => a.DefaultFee)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Seat>()
                .Property(s => s.BasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Flight>()
                .Property(f => f.BasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TotalPrice)
                .HasColumnType("decimal(18,2)");

            #endregion


            #region Relacionamentos e DeleteBehavior

            // Voo — Aeroporto de Origem
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.OriginAirport)
                .WithMany()
                .HasForeignKey(f => f.OriginAirportId)
                .OnDelete(DeleteBehavior.Restrict);


            // Voo — Aeroporto de Destino
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DestinationAirport)
                .WithMany()
                .HasForeignKey(f => f.DestinationAirportId)
                .OnDelete(DeleteBehavior.Restrict);


            // Voo — Aeronave
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Aircraft)
                .WithMany()
                .HasForeignKey(f => f.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);


            // Voo — Gate
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Gate)
                .WithMany()
                .HasForeignKey(f => f.GateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // Voo — Companhia Aérea
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Airline)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.AirlineId)
                .OnDelete(DeleteBehavior.Restrict);


            // Assento — Aeronave
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Aircraft)
                .WithMany(a => a.Seats)
                .HasForeignKey(s => s.AircraftId)
                .OnDelete(DeleteBehavior.Cascade);


            // Assento — Voo
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Seats)
                .HasForeignKey(s => s.FlightId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // Bilhete — Passageiro
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Passenger)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PassengerId)
                .OnDelete(DeleteBehavior.Restrict);


            // Bilhete — Voo
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Flight)
                .WithMany()
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Restrict);


            // Bilhete — Assento
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);


            // Bilhete — User (quem criou)
            modelBuilder.Entity<Ticket>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // Passageiro — User
            modelBuilder.Entity<Passenger>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ForumTopic — User
            modelBuilder.Entity<ForumTopic>()
                .HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // ForumComment — ForumTopic
            modelBuilder.Entity<ForumComment>()
                .HasOne(c => c.ForumTopic)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.ForumTopicId)
                .OnDelete(DeleteBehavior.Cascade);


            // ForumComment — User
            modelBuilder.Entity<ForumComment>()
                .HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            // Notificação — User (destinatário)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // Cartão de Embarque — Bilhete
            modelBuilder.Entity<BoardingPass>()
                .HasOne(bp => bp.Ticket)
                .WithOne(t => t.BoardingPass)
                .HasForeignKey<BoardingPass>(bp => bp.TicketId)
                .IsRequired(false);


            // Comunicação — Voo
            modelBuilder.Entity<FlightCommunication>()
                .HasOne(c => c.Flight)
                .WithMany()
                .HasForeignKey(c => c.FlightId)
                .OnDelete(DeleteBehavior.Restrict);


            // Comunicação — Funcionário/User que enviou
            modelBuilder.Entity<FlightCommunication>()
                .HasOne(c => c.SentByUser)
                .WithMany()
                .HasForeignKey(c => c.SentByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion
        }


        #region SaveChanges - Soft Delete + Auditoria Automática

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // Garante que o EF conhece todas as alterações atuais.
            ChangeTracker.DetectChanges();

            // ---------------------------------------------------------
            // 1. Capturar auditoria ANTES de alterar Deleted
            //    para Modified por causa do Soft Delete.
            // ---------------------------------------------------------

            var auditLogs = CreateAutomaticAuditLogs();


            // ---------------------------------------------------------
            // 2. Aplicar Soft Delete
            // ---------------------------------------------------------

            var deletedEntries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.State == EntityState.Deleted &&
                    e.Entity is ISoftDelete)
                .ToList();

            foreach (var entry in deletedEntries)
            {
                entry.State = EntityState.Modified;

                ((ISoftDelete)entry.Entity).WasDeleted = true;
            }


            // ---------------------------------------------------------
            // 3. Adicionar os registos de auditoria
            // ---------------------------------------------------------

            if (auditLogs.Count > 0)
            {
                AuditLogs.AddRange(auditLogs);
            }


            // ---------------------------------------------------------
            // 4. Guardar tudo
            // ---------------------------------------------------------

            return await base.SaveChangesAsync(
                cancellationToken);
        }


        // =============================================================
        // CRIAR AUDITORIA AUTOMÁTICA
        // =============================================================

        private List<AuditLog> CreateAutomaticAuditLogs()
        {
            var logs = new List<AuditLog>();

            var userId = GetCurrentUserId();


            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.Entity is not AuditLog &&
                    e.State is EntityState.Added
                        or EntityState.Modified
                        or EntityState.Deleted)
                .ToList();


            foreach (var entry in entries)
            {
                var entityName =
                    entry.Metadata.ClrType.Name;


                string action;

                switch (entry.State)
                {
                    case EntityState.Added:
                        action = "Create";
                        break;

                    case EntityState.Modified:
                        action = "Update";
                        break;

                    case EntityState.Deleted:
                        action = "Delete";
                        break;

                    default:
                        continue;
                }


                var oldValues = new StringBuilder();
                var newValues = new StringBuilder();


                // -----------------------------------------------------
                // PROPRIEDADES
                // -----------------------------------------------------

                foreach (var property in entry.Properties)
                {
                    var propertyName =
                        property.Metadata.Name;


                    // Não guardar informação sensível.
                    if (IsSensitiveProperty(propertyName))
                        continue;


                    // UPDATE:
                    // guardar apenas propriedades realmente alteradas.
                    if (entry.State == EntityState.Modified)
                    {
                        if (!property.IsModified)
                            continue;

                        var oldValue =
                            FormatValue(property.OriginalValue);

                        var newValue =
                            FormatValue(property.CurrentValue);


                        if (oldValue == newValue)
                            continue;


                        AppendValue(
                            oldValues,
                            propertyName,
                            oldValue);

                        AppendValue(
                            newValues,
                            propertyName,
                            newValue);
                    }


                    // CREATE:
                    // não existe valor anterior.
                    else if (entry.State == EntityState.Added)
                    {
                        AppendValue(
                            newValues,
                            propertyName,
                            FormatValue(property.CurrentValue));
                    }


                    // DELETE:
                    // guardar os valores que existiam.
                    else if (entry.State == EntityState.Deleted)
                    {
                        AppendValue(
                            oldValues,
                            propertyName,
                            FormatValue(property.OriginalValue));
                    }
                }


                // Se for Modified mas nenhuma propriedade real
                // tiver mudado, não precisamos criar auditoria.
                if (entry.State == EntityState.Modified &&
                    oldValues.Length == 0 &&
                    newValues.Length == 0)
                {
                    continue;
                }


                // -----------------------------------------------------
                // RELAÇÕES ESPECIAIS
                // -----------------------------------------------------

                int? flightId =
                    GetIntegerProperty(entry, "FlightId");

                int? ticketId =
                    GetTicketId(entry);


                // -----------------------------------------------------
                // DESCRIÇÃO
                // -----------------------------------------------------

                var description =
                    action switch
                    {
                        "Create" =>
                            $"{entityName} criado.",

                        "Update" =>
                            $"{entityName} atualizado.",

                        "Delete" =>
                            $"{entityName} eliminado.",

                        _ =>
                            $"{entityName} alterado."
                    };


                // -----------------------------------------------------
                // CRIAR AUDIT LOG
                // -----------------------------------------------------

                logs.Add(new AuditLog
                {
                    UserId = userId,

                    Action = action,

                    Category = entityName,

                    Description = description,

                    FlightId = flightId,

                    TicketId = ticketId,

                    OldValue =
                        LimitText(
                            oldValues.ToString()),

                    NewValue =
                        LimitText(
                            newValues.ToString()),

                    CreatedAt = DateTime.UtcNow
                });
            }


            return logs;
        }


        // =============================================================
        // UTILIZADOR ATUAL
        // =============================================================

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor?
                .HttpContext?
                .User?
                .FindFirstValue(
                    ClaimTypes.NameIdentifier);
        }


        // =============================================================
        // TICKET ID
        // =============================================================

        private static int? GetTicketId(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            // Se a própria entidade for Ticket,
            // usamos a chave Id.
            if (entry.Entity is Ticket)
            {
                return GetIntegerProperty(
                    entry,
                    "Id");
            }


            // Para entidades relacionadas com Ticket,
            // tentamos TicketId.
            return GetIntegerProperty(
                entry,
                "TicketId");
        }


        // =============================================================
        // OBTER PROPRIEDADE INTEIRA
        // =============================================================

        private static int? GetIntegerProperty(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            string propertyName)
        {
            var property =
                entry.Properties
                    .FirstOrDefault(p =>
                        p.Metadata.Name == propertyName);

            if (property == null)
                return null;


            var value =
                entry.State == EntityState.Deleted
                    ? property.OriginalValue
                    : property.CurrentValue;


            if (value == null)
                return null;


            if (value is int intValue)
                return intValue;


            if (int.TryParse(
                value.ToString(),
                out var result))
            {
                return result;
            }


            return null;
        }


        // =============================================================
        // PROPRIEDADES SENSÍVEIS
        // =============================================================

        private static bool IsSensitiveProperty(
            string propertyName)
        {
            var sensitiveProperties =
                new[]
                        {
                    "PasswordHash",
                    "SecurityStamp",
                    "ConcurrencyStamp",
                    "AuthenticatorKey",
                    "Token",
                    "AccessToken",
                    "RefreshToken",
                    "Password",
                    "Secret",
                    "ApiKey"
                };


            return sensitiveProperties.Any(
                sensitive =>
                    propertyName.Contains(
                        sensitive,
                        StringComparison.OrdinalIgnoreCase));
        }


        // =============================================================
        // FORMATAR VALORES
        // =============================================================

        private static string FormatValue(
            object? value)
        {
            if (value == null)
                return "null";


            if (value is DateTime dateTime)
            {
                return dateTime.ToString(
                    "yyyy-MM-dd HH:mm:ss");
            }


            if (value is bool boolean)
            {
                return boolean
                    ? "true"
                    : "false";
            }


            return value.ToString() ?? string.Empty;
        }


        // =============================================================
        // ADICIONAR VALOR AO TEXTO
        // =============================================================

        private static void AppendValue(
            StringBuilder builder,
            string property,
            string value)
        {
            if (builder.Length > 0)
            {
                builder.Append(" | ");
            }


            builder.Append(property);
            builder.Append(": ");
            builder.Append(value);
        }


        // =============================================================
        // LIMITAR A 500 CARACTERES
        // =============================================================

        private static string? LimitText(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;


            const int maxLength = 500;


            if (value.Length <= maxLength)
                return value;


            return value[..497] + "...";
        }

        #endregion
    }
}