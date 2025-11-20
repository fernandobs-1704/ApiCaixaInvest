using ApiCaixaInvest.Domain.Models;
using Swashbuckle.AspNetCore.Filters;

public class ProdutosPorRiscoExample : IExamplesProvider<IEnumerable<ProdutoInvestimento>>
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
                Id = 102,
                Nome = "LCI Caixa 2 Anos",
                Tipo = "LCI",
                RentabilidadeAnual = 0.115m,
                Risco = "Baixo",
                PrazoMinimoMeses = 24,
                LiquidezDias = 180
            }
        };
    }
}
