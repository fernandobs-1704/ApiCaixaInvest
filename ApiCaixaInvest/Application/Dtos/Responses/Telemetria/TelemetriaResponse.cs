namespace ApiCaixaInvest.Application.Dtos.Responses.Telemetria;

/// <summary>
/// Resposta completa do endpoint de telemetria.
/// </summary>
public class TelemetriaResponse
{
    public List<TelemetriaServicoResponse> Servicos { get; set; } = new();
    public TelemetriaPeriodoResponse Periodo { get; set; } = new();
}
