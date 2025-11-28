using System;
using System.Collections.Generic;

namespace Kakeibo.AppHost.Web.Models;

public class PlanoAnual
{
    public decimal PlanoAnualId { get; set; }

    public string UserId { get; set; } = null!;

    public int Ano { get; set; }

    public int Mes { get; set; }

    public string Objetivo { get; set; } = null!;

    public decimal MetaEmValor { get; set; }

    public string? Observacoes { get; set; }

    public string? MetaAtingida { get; set; }
}
