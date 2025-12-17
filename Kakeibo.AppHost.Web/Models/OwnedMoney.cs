using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kakeibo.AppHost.Web.Models;

[Table("OwnedMoney")]
public class OwnedMoney
{
    [Key]
    [Column(TypeName = "numeric(18,0)")]
    public decimal OwnedMoneyID { get; set; }

    [Required, StringLength(2)]
    public string Mes { get; set; } = string.Empty;

    [Required, StringLength(4)]
    public string Ano { get; set; } = string.Empty;

    [StringLength(450)]
    public string? UserID { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal MonthlyTotalIncome { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal TotalFixedExpenses { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal IncomeMinusFixedExp { get; set; }
}