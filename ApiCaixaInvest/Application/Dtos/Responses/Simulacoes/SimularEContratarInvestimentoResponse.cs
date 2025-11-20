using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;

/// <summary>
/// Resposta completa quando a API simula e efetiva o investimento em uma única operação.
/// </summary>
[NotMapped]
public class SimularEContratarInvestimentoResponse
{
    /// <summary>
    /// Indica se a operação (simulação + contratação) foi concluída com sucesso.
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem amigável de retorno para o consumidor da API.
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do cliente que realizou a operação.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Detalhes da simulação realizada.
    /// </summary>
    public SimularInvestimentoResponse Simulacao { get; set; } = new();

    /// <summary>
    /// Investimento efetivado com base na simulação (último investimento registrado).
    /// </summary>
    public InvestimentoHistoricoResponse? Investimento { get; set; }

    /// <summary>
    /// Perfil de risco recalculado após a contratação do investimento.
    /// </summary>
    public PerfilRiscoResponse PerfilRisco { get; set; } = new();
}
