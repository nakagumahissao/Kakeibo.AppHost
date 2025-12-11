using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kakeibo.AppHost.Web.Models;

public partial class Despesa
{
    [Key]
    public int DespesaId { get; set; }

    public int TipoDespesaId { get; set; }

    public string?  TipoDespesaNome { get; set; }

    public string NomeDespesa { get; set; } = null!;

    public string? UserId { get; set; }
}
