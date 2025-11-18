namespace ApiCaixaInvest.Models;

public class PerfilCliente
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Perfil { get; set; } = string.Empty; // Conservador, Moderado, Agressivo
    public int Pontuacao { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
}
