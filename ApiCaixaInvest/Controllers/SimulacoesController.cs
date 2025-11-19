using ApiCaixaInvest.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class SimulacoesController : ControllerBase
{
    private readonly IInvestmentSimulationService _simulationService;
    private readonly ISimulacoesConsultaService _simulacoesConsultaService;

    public SimulacoesController(
        IInvestmentSimulationService simulationService,
        ISimulacoesConsultaService simulacoesConsultaService)
    {
        _simulationService = simulationService;
        _simulacoesConsultaService = simulacoesConsultaService;
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
        var simulacoes = await _simulacoesConsultaService.ObterHistoricoAsync();
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
        var resumo = await _simulacoesConsultaService.ObterResumoPorProdutoDiaAsync();
        return Ok(resumo);
    }
}
