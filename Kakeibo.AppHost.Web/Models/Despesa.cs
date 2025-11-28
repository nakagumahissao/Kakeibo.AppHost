using System;
using System.Collections.Generic;

namespace Kakeibo.AppHost.Web.Models;

public partial class Despesa
{
    public int DespesaId { get; set; }

    public int TipoDespesaId { get; set; }

    public string?  TipoDespesaNome { get; set; }

    public string NomeDespesa { get; set; } = null!;
}
