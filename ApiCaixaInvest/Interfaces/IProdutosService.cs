using ApiCaixaInvest.Dtos.Responses.Produtos;

namespace ApiCaixaInvest.Interfaces;

public interface IProdutosService
{
    Task<IReadOnlyList<ProdutoRecomendadoResponse>> ObterProdutosRecomendadosAsync(string perfil);
}
