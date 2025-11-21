using ApiCaixaInvest.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

[NotMapped]
public class PerfilRiscoBasicoResponse
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
    /// Pontuação numérica usada para classificação (0–100+).
    /// </summary>
    public int Pontuacao { get; set; }

    /// <summary>
    /// Descrição detalhada do perfil, conforme solicitado no enunciado.
    /// </summary>
    public string Descricao { get; set; } = string.Empty;
}
