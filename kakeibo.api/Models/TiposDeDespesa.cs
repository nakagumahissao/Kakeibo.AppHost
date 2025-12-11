using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kakeibo.api
{
    [Table("TiposDeDespesa")]
    public class TiposDeDespesa
    {
        public TiposDeDespesa()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Tipo Despesa ID is required")]
        [Display(Name = "Tipo Despesa ID")]
        public required decimal TipoDespesaID { get; set; } // int, not null

        [MaxLength(30)]
        [StringLength(30)]
        [Required(ErrorMessage = "Tipo De Despesa is required")]
        [Display(Name = "Tipo De Despesa")]
        public required string TipoDeDespesa { get; set; } // nvarchar(30), not null

        [MaxLength(128)]
        [StringLength(128)]
        [Display(Name = "User ID")]
        public string? UserID { get; set; }
    }
}
