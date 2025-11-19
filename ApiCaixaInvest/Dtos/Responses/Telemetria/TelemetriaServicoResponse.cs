namespace ApiCaixaInvest.Dtos.Responses.Telemetria;

/// <summary>
/// Resumo de chamadas de um serviço da API dentro de um período.
/// </summary>
public class TelemetriaServicoResponse
{
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeChamadas { get; set; }
    public long MediaTempoRespostaMs { get; set; }
}
