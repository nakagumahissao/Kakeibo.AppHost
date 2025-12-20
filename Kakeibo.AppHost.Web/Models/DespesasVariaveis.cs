namespace Kakeibo.AppHost.Web.Models;

public class DespesasVariaveis
{
    public string Ano { get; set; } = string.Empty;
    public string Mes { get; set; } = string.Empty;

    public string? UserID { get; set; }

    public string NomeDespesa { get; set; } = string.Empty;

    // Use decimal for financial values to avoid rounding errors
    public decimal TotalDestaCategoria { get; set; }
}
