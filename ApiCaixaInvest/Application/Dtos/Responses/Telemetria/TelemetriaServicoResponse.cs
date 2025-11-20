using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Telemetria;

/// <summary>
/// Resumo de chamadas de um serviço da API dentro de um período.
/// </summary>
[NotMapped]
public class TelemetriaServicoResponse
{
    /// <summary>
    /// Nome do serviço/end-point (ex.: GET /api/simulacoes).
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade total de chamadas registradas para o serviço.
    /// </summary>
    public int QuantidadeChamadas { get; set; }

    /// <summary>
    /// Tempo médio de resposta em milissegundos.
    /// </summary>
    public long MediaTempoRespostaMs { get; set; }
}
