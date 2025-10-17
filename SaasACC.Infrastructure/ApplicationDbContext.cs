using Microsoft.EntityFrameworkCore;
using SaasACC.Model.Entities;

namespace SaasACC.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Comercio> Comercios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<CuentaCorriente> CuentasCorrientes { get; set; }
    public DbSet<Movimiento> Movimientos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SaasACCDb;Trusted_Connection=true;TrustServerCertificate=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar entidades
        ConfigurarComercio(modelBuilder);
        ConfigurarUsuario(modelBuilder);
        ConfigurarCliente(modelBuilder);
        ConfigurarCuentaCorriente(modelBuilder);
        ConfigurarMovimiento(modelBuilder);

        // Filtros globales para soft delete
        modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.Activo);
        modelBuilder.Entity<CuentaCorriente>().HasQueryFilter(cc => cc.Activo);
        modelBuilder.Entity<Movimiento>().HasQueryFilter(m => m.Activo);
    }

    private static void ConfigurarComercio(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comercio>(entity =>
        {
            entity.ToTable("Comercios");
            entity.HasKey(c => c.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Telefono)
                .HasMaxLength(20);

            entity.Property(c => c.Direccion)
                .HasMaxLength(300);

            entity.Property(c => c.Logo)
                .HasMaxLength(500);

            entity.HasIndex(c => c.Email).IsUnique();
        });
    }

    private static void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(u => u.Rol)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Usuario");

            entity.HasOne(u => u.Comercio)
                .WithMany(c => c.Usuarios)
                .HasForeignKey(u => u.ComercioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(u => u.Email).IsUnique();
        });
    }

    private static void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Apellido)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.Telefono)
                .HasMaxLength(20);

            entity.Property(c => c.Direccion)
                .HasMaxLength(300);

            entity.Property(c => c.DNI)
                .HasMaxLength(20);

            entity.HasOne(c => c.Comercio)
                .WithMany(co => co.Clientes)
                .HasForeignKey(c => c.ComercioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.CuentaCorriente)
                .WithOne(cc => cc.Cliente)
                .HasForeignKey<CuentaCorriente>(cc => cc.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => new { c.ComercioId, c.Email }).IsUnique();
        });
    }

    private static void ConfigurarCuentaCorriente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuentaCorriente>(entity =>
        {
            entity.ToTable("CuentasCorrientes");
            entity.HasKey(cc => cc.Id);

            entity.Property(cc => cc.LimiteCredito)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(cc => cc.Observaciones)
                .HasMaxLength(500);

            entity.HasMany(cc => cc.Movimientos)
                .WithOne(m => m.CuentaCorriente)
                .HasForeignKey(m => m.CuentaCorrienteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cc => cc.ClienteId).IsUnique();
        });
    }

    private static void ConfigurarMovimiento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.ToTable("Movimientos");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.TipoMovimiento)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(m => m.Importe)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(m => m.Descripcion)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(m => m.Comprobante)
                .HasMaxLength(100);

            entity.Property(m => m.ObservacionesPago)
                .HasMaxLength(300);

            entity.HasIndex(m => m.CuentaCorrienteId);
            entity.HasIndex(m => m.FechaCreacion);
            entity.HasIndex(m => m.FechaVencimiento);
            entity.HasIndex(m => m.Pagado);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Actualizar automáticamente FechaModificacion
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.FechaModificacion = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}