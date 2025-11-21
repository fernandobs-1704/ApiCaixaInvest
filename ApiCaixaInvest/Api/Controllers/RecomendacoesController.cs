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
        private readonly IRiskProfileService _riskProfileService;

        public RecomendacoesController(
            IProdutosService produtosService,
            IRiskProfileService riskProfileService)
        {
            _produtosService = produtosService;
            _riskProfileService = riskProfileService;
        }

        [HttpGet("produtos/{perfil}")]
        [SwaggerOperation(
            Summary = "Retorna produtos recomendados com base no perfil de risco.",
            Description = "Perfis aceitos: Conservador, Moderado, Agressivo."
        )]
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


        [HttpGet("cliente/{clienteId:int}")]
        [SwaggerOperation(
            Summary = "Recomenda produtos automaticamente para um cliente.",
            Description = "Identifica o perfil de risco atual do cliente e retorna produtos adequados ao seu comportamento real."
        )]
        [SwaggerResponse(StatusCodes.Status200OK,
            "Recomendações geradas com sucesso.",
            typeof(IEnumerable<ProdutoRecomendadoResponse>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest,
            "Parâmetro clienteId inválido.")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized,
            "Token JWT ausente ou inválido.")]
        public async Task<ActionResult<IEnumerable<ProdutoRecomendadoResponse>>> GetRecomendacoesPorCliente(int clienteId)
        {
            if (clienteId <= 0)
                return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

            // 1) Calcula perfil dinâmico
            var perfil = await _riskProfileService.CalcularPerfilAsync(clienteId);

            // 2) Busca recomendações com base no perfil REAL
            var produtos = await _produtosService.ObterProdutosRecomendadosAsync(perfil.Perfil);

            return Ok(new
            {
                clienteId,
                perfilAtual = perfil.Perfil,
                pontuacao = perfil.Pontuacao,
                recomendacoes = produtos
            });
        }
    }
}
