using Microsoft.EntityFrameworkCore;
using PaypalPayment.Domain.Payments;

namespace PaypalPayment.Infrastructure.Persistence
{
    public class PypalPaymentDbContext : DbContext
    {
        public PypalPaymentDbContext(DbContextOptions<PypalPaymentDbContext> dbContextOptions)
            : base(dbContextOptions)
        {

        }

        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.ProviderOrderId)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.HasIndex(p => p.ProviderOrderId)
                      .IsUnique();

                entity.Property(p => p.Amount)
                      .HasPrecision(18, 2);

                entity.Property(p => p.Currency)
                      .IsRequired()
                      .HasMaxLength(3);

                // Map enum PaymentStatus as int
                entity.Property(p => p.Status)
                      .HasConversion<int>()
                      .IsRequired();

                entity.Property(p => p.Description)
                      .HasMaxLength(256);

                entity.Property(p => p.PayerEmail)
                      .HasMaxLength(256);

                entity.Property(p => p.PayerId)
                      .HasMaxLength(128);

                entity.Property(p => p.CreatedAt)
                      .IsRequired();
            });
        }
    }
}
