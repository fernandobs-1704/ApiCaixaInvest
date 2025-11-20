namespace ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

/// <summary>
/// Representa o perfil de risco calculado para um cliente.
/// </summary>
public class PerfilRiscoResponse
{
    public int ClienteId { get; set; }
    public string Perfil { get; set; } = string.Empty; // Conservador, Moderado, Agressivo
    public int Pontuacao { get; set; }                 // 0 a 100
    public DateTime UltimaAtualizacao { get; set; }    // Horário em Brasília
    public string Explicacao { get; set; } = string.Empty;
}
