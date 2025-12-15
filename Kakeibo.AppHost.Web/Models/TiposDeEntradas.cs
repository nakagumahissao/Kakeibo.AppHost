using Kakeibo.AppHost.Web;
using System;
using Kakeibo.AppHost.Web.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kakeibo.AppHost.Web.Models;

public class TiposDeEntradas
{
    [Key]
    public int TipoDeEntradaId { get; set; }

    [Required(ErrorMessageResourceName = "Campo Obrigatório", ErrorMessageResourceType = typeof(SharedResources))]
    public required string TipoDeEntrada { get; set; } = null!;

    public string? UserId { get; set; }
    }
