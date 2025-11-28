using System;
using System.Collections.Generic;

namespace Kakeibo.AppHost.Web.Models;

public class Resultado
{
    public decimal ResultadoId { get; set; }

    public int Ano { get; set; }

    public int Mes { get; set; }

    public string? UserId { get; set; }

    public decimal ATotalEntradas { get; set; }

    public decimal BTotalDespesasFixas { get; set; }

    public decimal? CSubTotalAB { get; set; }

    public decimal DTotalDespesasVariaveis { get; set; }

    public decimal? ESubTotalBD { get; set; }

    public decimal? ResultadoParaProxMesAE { get; set; }
}
