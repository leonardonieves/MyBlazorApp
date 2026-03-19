using Microsoft.EntityFrameworkCore;
using MyBlazorApp.Models;

namespace MyBlazorApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Raffle> Raffles { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Winner> Winners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId);

            // Unique constraint on username and email
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Payments
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Required fields
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Stripe IDs
            entity.Property(e => e.StripeSessionId).HasMaxLength(255);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(255);
            entity.Property(e => e.StripeCustomerId).HasMaxLength(255);
            entity.Property(e => e.StripeInvoiceId).HasMaxLength(255);
            entity.Property(e => e.ProductId).HasMaxLength(255);
            entity.Property(e => e.PriceId).HasMaxLength(255);

            // Customer info
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);

            // Money
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);

            // Indexes for fast queries
            entity.HasIndex(e => e.StripeSessionId).IsUnique();
            entity.HasIndex(e => e.StripePaymentIntentId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Seed roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Basic" }
        );

        // Raffles
        modelBuilder.Entity<Raffle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.PrizeDetails).IsRequired();
            entity.Property(e => e.TicketPrice).HasPrecision(18, 2);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.StripePriceId).HasMaxLength(100);

            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DrawDate);
        });

        // Tickets
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BuyerEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BuyerName).HasMaxLength(100);
            entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StripePaymentIntentId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StripeSessionId).HasMaxLength(255);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.Raffle)
                .WithMany(r => r.Tickets)
                .HasForeignKey(e => e.RaffleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RaffleId);
            entity.HasIndex(e => e.BuyerEmail);
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasIndex(e => e.StripePaymentIntentId);
        });

        // Winners
        modelBuilder.Entity<Winner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Raffle)
                .WithOne(r => r.Winner)
                .HasForeignKey<Winner>(e => e.RaffleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Ticket)
                .WithOne(t => t.Winner)
                .HasForeignKey<Winner>(e => e.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.RaffleId).IsUnique();
            entity.HasIndex(e => e.TicketId).IsUnique();
        });
    }
}

