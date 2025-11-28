using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kakeibo.api
{
    [Table("Entradas")]
    public class Entradas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Entrada ID is required")]
        [Display(Name = "Entrada ID")]
        public decimal EntradaID { get; set; } // numeric(18,0), not null

        [MaxLength(4)]
        [StringLength(4)]
        [Required(ErrorMessage = "Ano is required")]
        [Display(Name = "Ano")]
        public string Ano { get; set; } // nvarchar(4), not null

        [MaxLength(2)]
        [StringLength(2)]
        [Required(ErrorMessage = "Mes is required")]
        [Display(Name = "Mes")]
        public string Mes { get; set; } // nvarchar(2), not null

        [MaxLength(450)]
        [StringLength(450)]
        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        public string UserID { get; set; } // nvarchar(450), not null

        [Required(ErrorMessage = "Tipo De Entrada ID is required")]
        [Display(Name = "Tipo De Entrada ID")]
        public int TipoDeEntradaID { get; set; } // int, not null

        [MaxLength(50)]
        [StringLength(50)]
        [Display(Name = "Descricao")]
        public string? Descricao { get; set; } // nvarchar(50), null

        [Required(ErrorMessage = "Valor Entrada is required")]
        [Display(Name = "Valor Entrada")]
        public decimal ValorEntrada { get; set; } // numeric(19,2), not null

        // dbo.Entradas.TipoDeEntradaID -> dbo.TiposDeEntradas.TipoDeEntradaID (FK_Entradas_TiposDeEntradas)
        [ForeignKey("TipoDeEntradaID")]
        public TiposDeEntradas TiposDeEntrada { get; set; }
    }
}
