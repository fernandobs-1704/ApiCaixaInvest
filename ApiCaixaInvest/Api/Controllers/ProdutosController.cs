using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly ApiCaixaInvestDbContext _db;

        public ProdutosController(ApiCaixaInvestDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lista os produtos de investimento disponíveis.",
            Description = "Consulta todos os produtos cadastrados no banco e utilizados nas simulações e recomendações."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lista de produtos retornada com sucesso.", typeof(IEnumerable<ProdutoInvestimento>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ProdutosListaExample))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
        public async Task<ActionResult<IEnumerable<ProdutoInvestimento>>> GetProdutos()
        {
            var produtos = await _db.ProdutosInvestimento
                .OrderBy(p => p.Risco)
                .ThenBy(p => p.Nome)
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Consulta um produto específico por identificador.",
            Description = "Retorna os detalhes de um produto cadastrado."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Produto encontrado.", typeof(ProdutoInvestimento))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ProdutoPorIdExample))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Produto não encontrado.")]
        public async Task<ActionResult<ProdutoInvestimento>> GetProdutoPorId(int id)
        {
            var produto = await _db.ProdutosInvestimento.FindAsync(id);

            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado." });

            return Ok(produto);
        }

        [HttpGet("risco/{risco}")]
        [SwaggerOperation(
            Summary = "Lista produtos filtrados por nível de risco.",
            Description = "Riscos aceitos: Baixo, Médio, Alto. Pesquisa é case-insensitive."
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lista de produtos filtrada retornada com sucesso.", typeof(IEnumerable<ProdutoInvestimento>))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ProdutosPorRiscoExample))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
        public async Task<ActionResult<IEnumerable<ProdutoInvestimento>>> GetProdutosPorRisco(string risco)
        {
            var riscoNormalizado = risco.Trim().ToLower();

            var produtos = await _db.ProdutosInvestimento
                .Where(p => p.Risco.ToLower() == riscoNormalizado)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return Ok(produtos);
        }
    }
}
