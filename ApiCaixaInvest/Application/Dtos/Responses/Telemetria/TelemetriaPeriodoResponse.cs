using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Telemetria;

/// <summary>
/// Período analisado na consulta de telemetria.
/// </summary>
[NotMapped]
public class TelemetriaPeriodoResponse
{
    /// <summary>
    /// Data inicial do período analisado.
    /// </summary>
    public DateOnly Inicio { get; set; }

    /// <summary>
    /// Data final do período analisado.
    /// </summary>
    public DateOnly Fim { get; set; }
}
