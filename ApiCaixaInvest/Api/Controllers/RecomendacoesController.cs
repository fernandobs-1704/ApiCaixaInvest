using ApiCaixaInvest.Application.Dtos.Responses.Produtos;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.Controllers
{
    [ApiController]
    [Route("api/recomendacoes")]
    [Authorize]
    public class RecomendacoesController : ControllerBase
    {
        private readonly IProdutosService _produtosService;

        public RecomendacoesController(IProdutosService produtosService)
        {
            _produtosService = produtosService;
        }

        [HttpGet("produtos/{perfil}")]
        [SwaggerOperation(
            Summary = "Retorna produtos recomendados com base no perfil de risco.",
            Description = "Perfis aceitos: **Conservador**, **Moderado**, **Agressivo**. " +
                          "O motor de recomendação combina o nível de risco do produto, " +
                          "a rentabilidade esperada e a adequação ao perfil informado."
        )]
        [SwaggerResponse(
            StatusCodes.Status200OK,
            "Lista de produtos recomendados retornada com sucesso.",
            typeof(IEnumerable<ProdutoRecomendadoResponse>))]
        [SwaggerResponseExample(
            StatusCodes.Status200OK,
            typeof(ProdutosRecomendadosExample))]
        [SwaggerResponse(
            StatusCodes.Status400BadRequest,
            "Perfil informado inválido (não é Conservador, Moderado ou Agressivo).")]
        [SwaggerResponse(
            StatusCodes.Status401Unauthorized,
            "Token JWT ausente ou inválido.")]
        public async Task<ActionResult<IEnumerable<ProdutoRecomendadoResponse>>> GetProdutosRecomendados(string perfil)
        {
            try
            {
                var produtos = await _produtosService.ObterProdutosRecomendadosAsync(perfil);
                return Ok(produtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
