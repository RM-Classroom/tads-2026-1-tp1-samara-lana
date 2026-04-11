using LocadoraVeiculosApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculosApi.Data
{
    public class LocadoraContext : DbContext
    {
        public LocadoraContext(DbContextOptions<LocadoraContext> options) : base(options)
        {
        }

        public DbSet<Fabricante> Fabricantes { get; set; }
        public DbSet<CategoriaVeiculo> CategoriasVeiculo { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Aluguel> Alugueis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CPF)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Veiculo>()
                .HasIndex(v => v.Placa)
                .IsUnique();

            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.Fabricante)
                .WithMany(f => f.Veiculos)
                .HasForeignKey(v => v.FabricanteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.CategoriaVeiculo)
                .WithMany(c => c.Veiculos)
                .HasForeignKey(v => v.CategoriaVeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aluguel>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Alugueis)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aluguel>()
                .HasOne(a => a.Veiculo)
                .WithMany(v => v.Alugueis)
                .HasForeignKey(a => a.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Aluguel>()
                .Property(a => a.Status)
                .HasConversion<string>();
        }
    }
}