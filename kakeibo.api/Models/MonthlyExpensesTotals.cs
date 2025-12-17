using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kakeibo.api.Models;

[Table("MonthlyExpensesTotals")]
public class MonthlyExpensesTotals
{
    [Key]
    [Column(TypeName = "numeric(18,0)")]
    public decimal MonthlyRecordID { get; set; }

    [Required, StringLength(4)]
    public string Ano { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string Mes { get; set; } = string.Empty;

    [StringLength(450)]
    public string? UserID { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal BankWithdrawals { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal FoodCosts { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal OtherExpenses { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal MonthlyTotal { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal SaldoParaMesSeguinte { get; set; }
}
