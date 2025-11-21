using ApiCaixaInvest.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

/// <summary>
/// Resultado do cálculo de perfil de risco do cliente.
/// </summary>
[NotMapped]
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
    /// Pontuação numérica usada para classificação (0–100+).
    /// </summary>
    public int Pontuacao { get; set; }

    /// <summary>
    /// Descrição detalhada do perfil, conforme solicitado no enunciado.
    /// </summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da última atualização do perfil de risco.
    /// </summary>
    public DateTime UltimaAtualizacao { get; set; }

    /// <summary>
    /// Matriz de tendência dos próximos perfis,
    /// calculada via modelo Markoviano estático.
    /// Exemplo:
    /// {
    ///   "Conservador": 0.80,
    ///   "Moderado": 0.18,
    ///   "Agressivo": 0.02
    /// }
    /// </summary>
    public Dictionary<string, double>? TendenciaPerfis { get; set; }

    /// <summary>
    /// Próximo perfil mais provável, considerando a matriz de transição.
    /// Ex.: "Moderado" ou "Agressivo".
    /// </summary>
    public string? ProximoPerfilProvavel { get; set; }
}
