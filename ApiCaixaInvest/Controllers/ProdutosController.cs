using ApiCaixaInvest.Dtos.Responses.Produtos;
using ApiCaixaInvest.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutosService _produtosService;

    public ProdutosController(IProdutosService produtosService)
    {
        _produtosService = produtosService;
    }

    /// <summary>
    /// Retorna a lista de produtos recomendados para um determinado perfil de risco.
    /// </summary>
    /// <remarks>
    /// Perfis aceitos: Conservador, Moderado, Agressivo.
    /// </remarks>
    [HttpGet("produtos-recomendados/{perfil}")]
    public async Task<ActionResult<IEnumerable<ProdutoRecomendadoResponse>>> GetProdutosRecomendados(string perfil)
    {
        try
        {
            var produtos = await _produtosService.ObterProdutosRecomendadosAsync(perfil);
            return Ok(produtos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
