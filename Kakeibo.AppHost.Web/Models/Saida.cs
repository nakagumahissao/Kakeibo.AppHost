using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kakeibo.AppHost.Web.Models;

public class Saida
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required(ErrorMessage = "Saida ID is required")]
    [Display(Name = "Saida ID")]
    public decimal SaidaID { get; set; } // numeric(18,0), not null

    [Display(Name = "Data Saida")]
    public DateTime? DataSaida { get; set; } // date, null

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

    [MaxLength(450)]
    [StringLength(450)]
    [Required(ErrorMessage = "User ID is required")]
    [Display(Name = "User ID")]
    public required string UserID { get; set; } // nvarchar(450), not null

    [Required(ErrorMessage = "Despesa ID is required")]
    [Display(Name = "Despesa ID")]
    public required int DespesaID { get; set; } // int, not null

    [MaxLength(50)]
    [StringLength(50)]
    [Display(Name = "Descricao")]
    public string? Descricao { get; set; } // nvarchar(50), null

    [MaxLength(80)]
    [StringLength(80)]
    [Required(ErrorMessage = "Nome Despesa is required")]
    [Display(Name = "Nome Despesa")]
    public required string NomeDespesa { get; set; } // nvarchar(80), not null

    [Required(ErrorMessage = "Valor Despesa is required")]
    [Display(Name = "Valor Despesa")]
    public required decimal ValorDespesa { get; set; } // numeric(19,2), not null
}
