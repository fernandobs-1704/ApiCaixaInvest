namespace ApiCaixaInvest.Dtos.Responses;

/// <summary>
/// Representa um resumo agregando telemetria por serviço e dia.
/// </summary>
public class TelemetriaResumoResponse
{
    public string Servico { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public int QuantidadeChamadas { get; set; }
    public long TempoMedioMs { get; set; }
}
