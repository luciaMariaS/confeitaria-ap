using Confeitaria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Confeitaria.Api.Dados;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Encomenda> Encomendas => Set<Encomenda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Nome).HasMaxLength(120).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.SenhaHash).HasMaxLength(100).IsRequired();
            e.Property(u => u.Papel).HasMaxLength(20).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Produto>(e =>
        {
            e.Property(p => p.Nome).HasMaxLength(120).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(500);
            e.Property(p => p.Preco).HasPrecision(10, 2);
            e.Property(p => p.Categoria).HasMaxLength(60).IsRequired();
        });

        modelBuilder.Entity<Encomenda>(e =>
        {
            e.Property(x => x.Observacoes).HasMaxLength(500);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.DataRetirada);

            e.HasOne(x => x.Produto)
                .WithMany(p => p.Encomendas)
                .HasForeignKey(x => x.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}