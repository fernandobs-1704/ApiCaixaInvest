using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Requests.Investimentos;

/// <summary>
/// Envelope para efetivar uma ou mais simulações de investimento
/// transformando-as em investimentos reais.
/// </summary>
[NotMapped]
public class EfetivarSimulacoesRequest
{
    /// <summary>
    /// Identificador do cliente dono das simulações.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Lista de IDs de simulações que o cliente deseja efetivar como investimentos reais.
    /// </summary>
    public List<int> SimulacaoIds { get; set; } = new();
}
