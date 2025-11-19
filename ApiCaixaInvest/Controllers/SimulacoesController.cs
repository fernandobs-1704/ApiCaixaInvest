using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Requests;
using ApiCaixaInvest.Dtos.Responses;
using ApiCaixaInvest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class SimulacoesController : ControllerBase
{
    private readonly InvestmentSimulationService _simulationService;
    private readonly ApiCaixaInvestDbContext _db;

    public SimulacoesController(
        InvestmentSimulationService simulationService,
        ApiCaixaInvestDbContext db)
    {
        _simulationService = simulationService;
        _db = db;
    }

    /// <summary>
    /// Executa uma simulação de investimento.
    /// </summary>
    /// <remarks>
    /// Realiza validação, seleção do produto e cálculo do valor final.
    /// </remarks>
    [HttpPost("simular-investimento")]
    public async Task<ActionResult<SimularInvestimentoResponse>> SimularInvestimento(
        [FromBody] SimularInvestimentoRequest request)
    {
        try
        {
            var result = await _simulationService.SimularAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro inesperado ao simular investimento." });
        }
    }

    /// <summary>
    /// Lista todo o histórico de simulações realizadas.
    /// </summary>
    /// <remarks>
    /// Retorna todas as simulações armazenadas no sistema.
    /// </remarks>
    [HttpGet("simulacoes")]
    public async Task<ActionResult<IEnumerable<SimulacaoHistoricoResponse>>> GetSimulacoes()
    {
        var simulacoes = await _db.Simulacoes
            .Include(s => s.ProdutoInvestimento)
            .OrderByDescending(s => s.DataSimulacao)
            .Select(s => new SimulacaoHistoricoResponse
            {
                Id = s.Id,
                ClienteId = s.ClienteId,
                Produto = s.ProdutoInvestimento != null ? s.ProdutoInvestimento.Nome : string.Empty,
                ValorInvestido = s.ValorInvestido,
                ValorFinal = s.ValorFinal,
                PrazoMeses = s.PrazoMeses,
                DataSimulacao = s.DataSimulacao
            })
            .ToListAsync();

        return Ok(simulacoes);
    }

    /// <summary>
    /// Retorna o resumo das simulações agrupadas por produto e data.
    /// </summary>
    /// <remarks>
    /// Fornece dados consolidados para análises e dashboards.
    /// </remarks>
    [HttpGet("simulacoes/por-produto-dia")]
    public async Task<ActionResult<IEnumerable<SimulacoesPorProdutoDiaResponse>>> GetSimulacoesPorProdutoDia()
    {
        var query = await _db.Simulacoes
            .Include(s => s.ProdutoInvestimento)
            .GroupBy(s => new
            {
                Produto = s.ProdutoInvestimento != null ? s.ProdutoInvestimento.Nome : string.Empty,
                Data = s.DataSimulacao.Date
            })
            .Select(g => new SimulacoesPorProdutoDiaResponse
            {
                Produto = g.Key.Produto,
                Data = g.Key.Data,
                QuantidadeSimulacoes = g.Count(),
                MediaValorFinal = g.Average(x => x.ValorFinal)
            })
            .OrderBy(r => r.Produto)
            .ThenBy(r => r.Data)
            .ToListAsync();

        return Ok(query);
    }
}
