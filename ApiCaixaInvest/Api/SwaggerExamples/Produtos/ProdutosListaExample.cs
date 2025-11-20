using ApiCaixaInvest.Domain.Models;
using Swashbuckle.AspNetCore.Filters;

public class ProdutosListaExample : IExamplesProvider<IEnumerable<ProdutoInvestimento>>
{
    public IEnumerable<ProdutoInvestimento> GetExamples()
    {
        return new List<ProdutoInvestimento>
        {
            new ProdutoInvestimento
            {
                Id = 101,
                Nome = "CDB Caixa Liquidez Diária",
                Tipo = "CDB",
                RentabilidadeAnual = 0.105m,
                Risco = "Baixo",
                PrazoMinimoMeses = 6,
                LiquidezDias = 1
            },
            new ProdutoInvestimento
            {
                Id = 105,
                Nome = "Tesouro IPCA+ 2035",
                Tipo = "Tesouro",
                RentabilidadeAnual = 0.13m,
                Risco = "Médio",
                PrazoMinimoMeses = 36,
                LiquidezDias = 30
            },
            new ProdutoInvestimento
            {
                Id = 106,
                Nome = "Fundo Multimercado XPTO",
                Tipo = "Fundo Multimercado",
                RentabilidadeAnual = 0.18m,
                Risco = "Alto",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            }
        };
    }
}
