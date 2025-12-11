using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kakeibo.api
{
    [Table("Resultados")]
    public class Resultados
    {
        public Resultados()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required(ErrorMessage = "Resultado ID is required")]
        [Display(Name = "Resultado ID")]
        public decimal ResultadoID { get; set; } // numeric(18,0), not null

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

        [MaxLength(128)]
        [StringLength(128)]
        [Display(Name = "User ID")]
        public string? UserID { get; set; }

        [Required(ErrorMessage = "A Total Entradas is required")]
        [Display(Name = "A Total Entradas")]
        public required decimal A_TotalEntradas { get; set; } // numeric(19,2), not null

        [Required(ErrorMessage = "B Total Despesas Fixas is required")]
        [Display(Name = "B Total Despesas Fixas")]
        public required decimal B_TotalDespesasFixas { get; set; } // numeric(19,2), not null

        [Display(Name = "C Sub Total A B")]
        public decimal? C_SubTotalA_B { get; set; } // numeric(19,2), null

        [Required(ErrorMessage = "D Total Despesas Variaveis is required")]
        [Display(Name = "D Total Despesas Variaveis")]
        public decimal? D_TotalDespesasVariaveis { get; set; } // numeric(19,2), not null

        [Display(Name = "E Sub Total B D")]
        public decimal? E_SubTotal_B_D { get; set; } // numeric(19,2), null

        [Display(Name = "Resultado Para Prox Mes A E")]
        public decimal? ResultadoParaProxMes_A_E { get; set; } // numeric(19,2), null
    }
}
