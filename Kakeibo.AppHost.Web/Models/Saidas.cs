using Kakeibo.AppHost.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kakeibo.AppHost.Web.Models;

public class Saidas
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Saida ID")]
    public decimal SaidaID { get; set; } // numeric(18,0), not null

    [Display(Name = "Data Saida")]
    public DateTime? DataSaida { get; set; } // date, null

    [MaxLength(4)]
    [StringLength(4)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Ano")]
    public required string Ano { get; set; } // nvarchar(4), not null

    [MaxLength(2)]
    [StringLength(2)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Mes")]
    public required string Mes { get; set; } // nvarchar(2), not null

    [MaxLength(450)]
    [StringLength(450)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "User ID")]
    public required string UserID { get; set; } // nvarchar(450), not null

    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "ID da Despesa", ResourceType = typeof(SharedResources))]
    public required int DespesaID { get; set; } // int, not null

    [MaxLength(50)]
    [StringLength(50)]
    [Display(Name = "Descrição", ResourceType = typeof(SharedResources))]
    public string? Descricao { get; set; } // nvarchar(50), null

    [MaxLength(80)]
    [StringLength(80)]
    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Nome da Despesa", ResourceType = typeof(SharedResources))]
    public required string NomeDespesa { get; set; } // nvarchar(80), not null

    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    [Display(Name = "Valor da Despesa", ResourceType = typeof(SharedResources))]
    public required decimal ValorDespesa { get; set; } // numeric(19,2), not null

    [JsonIgnore]
    public string? DataSaidaString { get; set; }
}
