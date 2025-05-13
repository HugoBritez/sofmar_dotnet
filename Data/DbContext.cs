using Microsoft.EntityFrameworkCore;
using Api.Models.Entities;



namespace Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<ArticuloLote> ArticuloLotes { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<Sububicacion> SubUbicaciones { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<SubCategoria> SubCategorias { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Iva> Ivas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar las relaciones entre entidades
            modelBuilder.Entity<ArticuloLote>()
                .HasOne(al => al.Articulo)
                .WithMany(a => a.ArticuloLotes)
                .HasForeignKey(al => al.AlArticulo);

            modelBuilder.Entity<ArticuloLote>()
                .HasOne(al => al.Deposito)
                .WithMany()
                .HasForeignKey(al => al.AlDeposito);

            modelBuilder.Entity<SubCategoria>()
                .HasOne(sc => sc.Categoria)
                .WithMany()
                .HasForeignKey(sc => sc.ScCategoria);
        }
    }
}
