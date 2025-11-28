using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kakeibo.api
{
    [Table("PlanoAnual")]
    public class PlanoAnual
    {
        public PlanoAnual()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Plano Anual ID is required")]
        [Display(Name = "Plano Anual ID")]
        public decimal PlanoAnualID { get; set; } // numeric(18,0), not null

        [MaxLength(450)]
        [StringLength(450)]
        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        public required string UserID { get; set; } // nvarchar(450), not null

        [MaxLength(4)]
        [StringLength(4)]
        [Required(ErrorMessage = "Ano is required")]
        [Display(Name = "Ano")]
        public required string Ano { get; set; } // nvarchar(4), not null

        [MaxLength(2)]
        [StringLength(2)]
        [Required(ErrorMessage = "Mes is required")]
        [Display(Name = "Mes")]
        public required string Mes { get; set; } // nvarchar(2), not null

        [MaxLength(255)]
        [StringLength(255)]
        [Required(ErrorMessage = "Objetivo is required")]
        [Display(Name = "Objetivo")]
        public required string Objetivo { get; set; } // nvarchar(255), not null

        [Required(ErrorMessage = "Meta Em Valor is required")]
        [Display(Name = "Meta Em Valor")]
        public required decimal MetaEmValor { get; set; } // numeric(19,2), not null

        [MaxLength]
        [Display(Name = "Observacoes")]
        public string? Observacoes { get; set; } // nvarchar(max), null

        [MaxLength(10)]
        [StringLength(10)]
        [Display(Name = "Meta Atingida")]
        public string? MetaAtingida { get; set; } // nchar(10), null
    }
}
