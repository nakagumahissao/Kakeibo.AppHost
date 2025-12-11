using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kakeibo.AppHost.Web.Models;

public class TiposDeEntradas
{
    [Key]
    public int TipoDeEntradaId { get; set; }

    [Required(ErrorMessage = "Este campo é obligatorio.")]
    public required string TipoDeEntrada { get; set; } = null!;

    public string? UserId { get; set; }
    }
