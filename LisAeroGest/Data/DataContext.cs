using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Tabela de aeroportos.
        /// </summary>
        public DbSet<Airport> Airports { get; set; }

        /// <summary>
        /// Tabela de companhias aéreas.
        /// </summary>
        public DbSet<Airline> Airlines { get; set; }

        /// <summary>
        /// Tabela de gates de embarque.
        /// </summary>
        public DbSet<Gate> Gates { get; set; }

        /// <summary>
        /// Tabela de aeronaves.
        /// </summary>
        public DbSet<Aircraft> Aircrafts { get; set; }

        /// <summary>
        /// Tabela de assentos.
        /// </summary>
        public DbSet<Seat> Seats { get; set; }

        /// <summary>
        /// Tabela de voos.
        /// </summary>
        public DbSet<Flight> Flights { get; set; }

        /// <summary>
        /// Tabela de passageiros.
        /// </summary>
        public DbSet<Passenger> Passengers { get; set; }

        /// <summary>
        /// Tabela de bilhetes confirmados.
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }

        /// <summary>
        /// Tabela de reservas temporárias de bilhetes.
        /// </summary>
        public DbSet<TicketTemp> TicketsTemp { get; set; }

        /// <summary>
        /// Tabela de tópicos do fórum interno.
        /// </summary>
        public DbSet<ForumTopic> ForumTopics { get; set; }

        /// <summary>
        /// Tabela de comentários do fórum interno.
        /// </summary>
        public DbSet<ForumComment> ForumComments { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Necessário para o Identity funcionar corretamente
            base.OnModelCreating(modelBuilder);

            #region Filtros Globais de Soft Delete
            // Registos com WasDeleted = true nunca aparecem nas queries
            modelBuilder.Entity<Airport>().HasQueryFilter(a => !a.WasDeleted);
            modelBuilder.Entity<Airline>().HasQueryFilter(a => !a.WasDeleted);
            modelBuilder.Entity<Gate>().HasQueryFilter(g => !g.WasDeleted);
            modelBuilder.Entity<Aircraft>().HasQueryFilter(a => !a.WasDeleted);
            modelBuilder.Entity<Seat>().HasQueryFilter(s => !s.WasDeleted);
            modelBuilder.Entity<Flight>().HasQueryFilter(f => !f.WasDeleted);
            modelBuilder.Entity<Passenger>().HasQueryFilter(p => !p.WasDeleted);
            modelBuilder.Entity<Ticket>().HasQueryFilter(t => !t.WasDeleted);
            modelBuilder.Entity<TicketTemp>().HasQueryFilter(t => !t.WasDeleted);
            modelBuilder.Entity<ForumTopic>().HasQueryFilter(f => !f.WasDeleted);
            modelBuilder.Entity<ForumComment>().HasQueryFilter(f => !f.WasDeleted);
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

            modelBuilder.Entity<TicketTemp>()
                .Property(t => t.Price)
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
                .WithMany()
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

            // BilheteTemp — Passageiro
            modelBuilder.Entity<TicketTemp>()
                .HasOne(t => t.Passenger)
                .WithMany()
                .HasForeignKey(t => t.PassengerId)
                .OnDelete(DeleteBehavior.Restrict);

            // BilheteTemp — Voo
            modelBuilder.Entity<TicketTemp>()
                .HasOne(t => t.Flight)
                .WithMany()
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            // BilheteTemp — Assento
            modelBuilder.Entity<TicketTemp>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // BilheteTemp — User
            modelBuilder.Entity<TicketTemp>()
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
            #endregion
        }

        #region Soft Delete Interceptor
        /// <summary>
        /// Interceta o SaveChangesAsync e converte operações de Delete
        /// em Update com WasDeleted = true, implementando o Soft Delete.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDelete);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                ((ISoftDelete)entry.Entity).WasDeleted = true;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}
