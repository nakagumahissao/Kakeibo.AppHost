using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Entradas")]
public class Entradas
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Entrada ID", ResourceType = typeof(SharedResources))]
    public decimal EntradaID { get; set; } // numeric(18,0), not null

    [MaxLength(4)]
    [StringLength(4)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Ano", ResourceType = typeof(SharedResources))]
    public required string Ano { get; set; } // nvarchar(4), not null

    [MaxLength(2)]
    [StringLength(2)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Mes", ResourceType = typeof(SharedResources))]
    public required string Mes { get; set; } // nvarchar(2), not null

    [MaxLength(450)]
    [StringLength(450)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "ID do Usuário", ResourceType = typeof(SharedResources))]
    public required string UserID { get; set; } // nvarchar(450), not null

    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Tipo De Entrada ID", ResourceType = typeof(SharedResources))]
    public int TipoDeEntradaID { get; set; } // int, not null

    [MaxLength(50)]
    [StringLength(50)]
    [Display(Name = "Descricao", ResourceType = typeof(SharedResources))]
    public string? Descricao { get; set; } // nvarchar(50), null

    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Valor Entrada", ResourceType = typeof(SharedResources))]
    public decimal ValorEntrada { get; set; } // numeric(19,2), not null

    // dbo.Entradas.TipoDeEntradaID -> dbo.TiposDeEntradas.TipoDeEntradaID (FK_Entradas_TiposDeEntradas)
    [ForeignKey("TipoDeEntradaID")]
    public TiposDeEntradas? TiposDeEntradas { get; set; }
}
