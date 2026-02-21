using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Entities; // Doğru yazım bu

namespace StockFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<FinanceTransaction> FinanceTransactions { get; set; }
    public DbSet<ProductAuditLog> ProductAuditLogs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CustomerMovement> CustomerMovements { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Para birimleri için hassas ayar (Decimal alanlar)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // Sipariş ve Müşteri İlişkisi
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

        // Sipariş ve Sipariş Kalemleri İlişkisi
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);
        
        modelBuilder.Entity<CustomerMovement>()
            .HasOne(cm => cm.Customer)
            .WithMany() // Müşterinin birçok hareketi olabilir
            .HasForeignKey(cm => cm.CustomerId);
    }
}