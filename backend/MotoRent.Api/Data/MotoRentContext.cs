
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.Models;

namespace MotoRent.Api.Data;

public class MotoRentContext : DbContext
{
    public MotoRentContext(DbContextOptions<MotoRentContext> options)
        : base(options)
    {
    }

    public DbSet<Moto> Motos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Moto>()
            .Property(m => m.PrecioPorDia)
            .HasPrecision(18, 2);
    }
}
