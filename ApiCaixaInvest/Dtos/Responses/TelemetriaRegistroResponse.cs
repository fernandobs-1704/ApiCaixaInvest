namespace ApiCaixaInvest.Dtos.Responses;

/// <summary>
/// Representa um registro individual de telemetria.
/// </summary>
public class TelemetriaRegistroResponse
{
    public int Id { get; set; }
    public string Servico { get; set; } = string.Empty;
    public long TempoRespostaMs { get; set; }
    public DateTime Data { get; set; }
}
