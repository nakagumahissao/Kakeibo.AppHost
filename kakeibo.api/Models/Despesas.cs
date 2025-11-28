using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace kakeibo.api
{
    [Table("Despesas")]
    public class Despesas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Despesa ID is required")]
        [Display(Name = "Despesa ID")]
        public required int DespesaID { get; set; } // int, not null

        [Required(ErrorMessage = "Tipo Despesa ID is required")]
        [Display(Name = "Tipo Despesa ID")]
        public required int TipoDespesaID { get; set; } // int, not null

        [MaxLength(80)]
        [StringLength(80)]
        [Required(ErrorMessage = "Nome Despesa is required")]
        [Display(Name = "Nome Despesa")]
        public required string NomeDespesa { get; set; } // nvarchar(80), not null

        // dbo.Despesas.TipoDespesaID -> dbo.TiposDeDespesa.TipoDespesaID (FK_Despesas_TiposDeDespesa)
        [JsonIgnore]
        [ForeignKey("TipoDespesaID")]
        public TiposDeDespesa TiposDeDespesa { get; set; }
    }
}
