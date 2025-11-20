namespace ApiCaixaInvest.Domain.Models;

public class TelemetriaRegistro
{
    public int Id { get; set; }
    public string Servico { get; set; } = string.Empty;
    public long TempoRespostaMs { get; set; }
    public DateTime Data { get; set; }
}
