namespace ApiCaixaInvest.Dtos.Responses;

public class SimularInvestimentoResponse
{
    public ProdutoResponse ProdutoValidado { get; set; } = new();
    public ResultadoSimulacaoResponse ResultadoSimulacao { get; set; } = new();
    public DateTime DataSimulacao { get; set; }
}

public class ProdutoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Rentabilidade { get; set; }
    public string Risco { get; set; } = string.Empty;
}

public class ResultadoSimulacaoResponse
{
    public decimal ValorFinal { get; set; }
    public decimal RentabilidadeEfetiva { get; set; }
    public int PrazoMeses { get; set; }
}
