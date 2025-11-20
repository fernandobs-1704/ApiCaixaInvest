using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Domain.Models;

/// <summary>
/// Armazena uma métrica individual de telemetria capturada pela API.
/// </summary>
[NotMapped]
public class TelemetriaRegistro
{
    /// <summary>
    /// Identificador único do registro de telemetria.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome do serviço ou endpoint monitorado.
    /// </summary>
    public string Servico { get; set; } = string.Empty;

    /// <summary>
    /// Tempo de resposta da requisição em milissegundos.
    /// </summary>
    public long TempoRespostaMs { get; set; }

    /// <summary>
    /// Momento em que o registro foi gerado.
    /// </summary>
    public DateTime Data { get; set; }
}
