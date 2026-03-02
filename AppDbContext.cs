namespace RealTimeFinancialMonitor;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var t = modelBuilder.Entity<Transaction>();

        t.HasKey(x => x.Id);

        t.Property(x => x.TransactionGuid)
         .IsRequired();

        t.Property(x => x.Amount)
         .HasPrecision(18, 2)
         .IsRequired();

        t.Property(x => x.Currency)
         .HasMaxLength(8)
         .IsRequired();

        t.Property(x => x.Status)
         .IsRequired()
         .HasConversion<string>();

        t.Property(x => x.Timestamp)
         .IsRequired();

        t.Property(x => x.CreatedAtUtc)
         .IsRequired();

        t.HasIndex(x => x.Timestamp);

        t.HasIndex(x => x.TransactionGuid)
         .IsUnique();

       

        base.OnModelCreating(modelBuilder);
    }
}
