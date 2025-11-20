using ApiCaixaInvest.Application.Dtos.Responses.Produtos;

namespace ApiCaixaInvest.Application.Interfaces;

public interface IProdutosService
{
    Task<IReadOnlyList<ProdutoRecomendadoResponse>> ObterProdutosRecomendadosAsync(string perfil);
}
