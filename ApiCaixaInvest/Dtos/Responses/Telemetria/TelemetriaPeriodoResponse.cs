namespace ApiCaixaInvest.Dtos.Responses.Telemetria;

/// <summary>
/// Período analisado na consulta de telemetria.
/// </summary>
public class TelemetriaPeriodoResponse
{
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
}
