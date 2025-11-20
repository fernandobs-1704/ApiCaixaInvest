using ApiCaixaInvest.Domain.Enum;

namespace ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

/// <summary>
/// Resultado do cálculo de perfil de risco do cliente.
/// </summary>
public class PerfilRiscoResponse
{
    /// <summary>
    /// Identificador do cliente.
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Perfil em formato texto: Conservador, Moderado, Agressivo.
    /// </summary>
    public string Perfil { get; set; } = string.Empty;

    /// <summary>
    /// Perfil fortemente tipado (enum).
    /// </summary>
    public PerfilRiscoTipoEnum PerfilTipo { get; set; }

    /// <summary>
    /// Pontuação numérica usada para classificação (0–100).
    /// </summary>
    public int Pontuacao { get; set; }

    /// <summary>
    /// Descrição do perfil, conforme solicitado no enunciado.
    /// </summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da última atualização do perfil de risco.
    /// </summary>
    public DateTime UltimaAtualizacao { get; set; }
}
