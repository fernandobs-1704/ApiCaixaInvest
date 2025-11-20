using ApiCaixaInvest.Domain.Models;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

[NotMapped]
public class ProdutoPorIdExample : IExamplesProvider<ProdutoInvestimento>
{
    public ProdutoInvestimento GetExamples()
    {
        return new ProdutoInvestimento
        {
            Id = 101,
            Nome = "CDB Caixa Liquidez Diária",
            Tipo = "CDB",
            RentabilidadeAnual = 0.105m,
            Risco = "Baixo",
            PrazoMinimoMeses = 6,
            LiquidezDias = 1
        };
    }
}
