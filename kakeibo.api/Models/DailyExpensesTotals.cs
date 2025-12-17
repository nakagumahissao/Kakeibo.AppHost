using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ykakeibo.api.Models;

[Table("DailyExpensesTotals")]
public class DailyExpensesTotals
{
    [Key]
    [Column(TypeName = "numeric(18,0)")]
    public decimal DailyRecordID { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime DataSaida { get; set; }

    [Required, StringLength(4)]
    public string Ano { get; set; } = string.Empty;

    [Required, StringLength(2)]
    public string Mes { get; set; } = string.Empty;

    [StringLength(450)]
    public string? UserID { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal PrevSaldoBancario { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal BankWithdrawals { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal FoodCosts { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal OtherExpenses { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal DailyTotal { get; set; }

    [Column(TypeName = "numeric(19,2)")]
    public decimal SaldoBancarioPrevisto { get; set; }
}
