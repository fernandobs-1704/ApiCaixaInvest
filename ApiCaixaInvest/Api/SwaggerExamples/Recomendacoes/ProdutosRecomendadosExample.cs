using ApiCaixaInvest.Application.Dtos.Responses.Produtos;
using Swashbuckle.AspNetCore.Filters;

public class ProdutosRecomendadosExample : IExamplesProvider<IEnumerable<ProdutoRecomendadoResponse>>
{
    public IEnumerable<ProdutoRecomendadoResponse> GetExamples()
    {
        return new List<ProdutoRecomendadoResponse>
        {
            new ProdutoRecomendadoResponse
            {
                Id = 101,
                Nome = "CDB Caixa Liquidez Diária",
                Tipo = "CDB",
                Rentabilidade = 0.105m,
                Risco = "Baixo"
            },
            new ProdutoRecomendadoResponse
            {
                Id = 105,
                Nome = "Tesouro IPCA+ 2035",
                Tipo = "Tesouro",
                Rentabilidade = 0.13m,
                Risco = "Médio"
            },
            new ProdutoRecomendadoResponse
            {
                Id = 106,
                Nome = "Fundo Multimercado XPTO",
                Tipo = "Fundo Multimercado",
                Rentabilidade = 0.18m,
                Risco = "Alto"
            }
        };
    }
}
