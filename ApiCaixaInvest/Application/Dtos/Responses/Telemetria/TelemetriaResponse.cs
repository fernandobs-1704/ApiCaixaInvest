using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Telemetria;

/// <summary>
/// Resposta completa do endpoint de telemetria, incluindo serviços e período.
/// </summary>
[NotMapped]
public class TelemetriaResponse
{
    /// <summary>
    /// Lista de serviços monitorados com suas métricas agregadas.
    /// </summary>
    public List<TelemetriaServicoResponse> Servicos { get; set; } = new();

    /// <summary>
    /// Período considerado na coleta dos dados de telemetria.
    /// </summary>
    public TelemetriaPeriodoResponse Periodo { get; set; } = new();
}
