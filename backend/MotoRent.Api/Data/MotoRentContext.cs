
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace MotoRent.Api.Data;

public class MotoRentContext : IdentityDbContext<Usuario>
{
    public MotoRentContext(DbContextOptions<MotoRentContext> options)
        : base(options)
    {
    }

    public DbSet<Moto> Motos { get; set; }
    //guardar clientes 
    public DbSet<Cliente> Clientes { get; set; }

    public DbSet<Alquiler> Alquileres { get; set; }
    //guarda los pagos
    public DbSet<Pago> Pagos { get; set; }
    //guarda las oredenes de pago
    public DbSet<OrdenPago> OrdenesPago { get; set; }

    //guardar las solicitud de alquiler 
    public DbSet<SolicitudAlquiler> SolicitudesAlquiler { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Moto>()
            .Property(m => m.PrecioPorDia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Alquiler>()
            .Property(a => a.PrecioPorDia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Pago>()
            .Property(p => p.Importe)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenPago>()
    .Property(o => o.ImporteTotal)
    .HasPrecision(18, 2);
    }

}
