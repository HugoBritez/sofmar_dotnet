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
        public DbSet<ListaPrecio> ListaPrecio { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Moneda> Moneda { get; set; }
        public DbSet<Venta> Venta { get; set;  }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }
        public DbSet<DetalleVentaBonificacion> DetalleVentaBonificacion { get; set; }
        public DbSet<DetalleArticulosEditado> DetalleArticulosEditados { get; set; }
        public DbSet<DetalleVentaVencimiento> DetalleVentaVencimiento { get; set; }
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<DetallePedido> DetallePedido { get; set;  }
        public DbSet<DetallePedidoFaltante> DetallePedidoFaltante { get; set; }
        public DbSet<AreaSecuencia> AreaSecuencia { get; set; }
        public DbSet<PedidosEstados> PedidoEstado { get; set; }
        public DbSet<Presupuesto> Presupuesto { get; set; }
        public DbSet<DetallePresupuesto> DetallePresupuesto { get; set; }
        public DbSet<PresupuestoObservacion> PresupuestoObservacion { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Zona> Zonas { get; set; }
        public DbSet<UnidadMedida> UnidadMedidas { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<InventarioAuxiliar> InventariosAuxiliares { get; set; }
        public DbSet<InventarioAuxiliarItems> InventarioAuxiliarItems { get; set; }

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
                .WithMany(d => d.ArticulosLote)
                .HasForeignKey(al => al.AlDeposito);

            modelBuilder.Entity<SubCategoria>()
                .HasOne(sc => sc.Categoria)
                .WithMany()
                .HasForeignKey(sc => sc.ScCategoria);
        }
    }
}
