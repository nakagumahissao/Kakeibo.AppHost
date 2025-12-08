using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace kakeibo.api
{
    [Table("TiposDeEntradas")]
    public class TiposDeEntradas
    {
        public TiposDeEntradas()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Tipo De Entrada ID is required")]
        [Display(Name = "Tipo De Entrada ID")]
        public required int TipoDeEntradaID { get; set; } // int, not null

        [MaxLength(80)]
        [StringLength(80)]
        [Required(ErrorMessage = "Tipo De Entrada is required")]
        [Display(Name = "Tipo De Entrada")]
        public required string TipoDeEntrada { get; set; } // nvarchar(80), not null

        [MaxLength(128)]
        [StringLength(128)]
        [Display(Name = "User ID")]
        public string? UserID { get; set; }
    }
}
