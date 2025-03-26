using device_drop_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace device_drop_backend.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<User>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<Category>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<Category>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<Product>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<Product>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<ProductVariant>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<Cart>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<Cart>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<CartItem>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<CartItem>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<Order>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            modelBuilder.Entity<Order>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}