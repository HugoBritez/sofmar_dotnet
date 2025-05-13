using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    public class ArticuloLote
    {
        [Column("al_codigo")]
        public uint AlCodigo { get; set; }

        [Column("al_articulo")]
        public uint AlArticulo { get; set;}

        [ForeignKey("AlArticulo")]
        public virtual Articulo? Articulo { get; set; }

        [Column("al_deposito")]
        public uint AlDeposito { get; set;}

        [ForeignKey("AlDeposito")]
        public virtual Deposito? Deposito { get; set; }

        [Column("al_lote")]
        public string AlLote { get; set;} = string.Empty;

        [Column("al_cantidad")]
        public decimal AlCantidad { get; set;}

        [Column("al_vencimiento")]
        public DateTime AlVencimiento { get; set;}

        [Column("al_pre_compra")]
        public decimal AlPreCompra { get; set; }

        [Column("al_origen")]
        public uint AlOrigen { get; set;}

        [Column("al_serie")]
        public string ALSerie { get; set;} = string.Empty;

        [Column("al_codbarra")]
        public string AlCodBarra { get; set;} = string.Empty;

        [Column("al_nrotalle")]
        public string AlNroTalle { get; set; } = string.Empty;

        [Column("al_color")]
        public uint AlColor { get; set;}

        [Column("al_talle")]
        public uint AlTalle { get; set; }

        [Column("al_registro")]
        public string AlRegistro { get; set;} = string.Empty;
        
    }
}