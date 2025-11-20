using ApiCaixaInvest.Api.SwaggerExamples.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SimulacoesController : ControllerBase
{
    private readonly ISimulacoesService _simulacoesService;

    public SimulacoesController(ISimulacoesService simulacoesService)
    {
        _simulacoesService = simulacoesService;
    }

    [HttpPost("simular-investimento")]
    [SwaggerOperation(
        Summary = "Simula um investimento.",
        Description = "Recebe os parâmetros de valor, prazo e tipo de produto, valida contra as regras de negócio " +
                      "e retorna o produto mais adequado e o resultado da simulação."
    )]
    [SwaggerRequestExample(typeof(SimularInvestimentoRequest), typeof(SimularInvestimentoRequestExample))]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Simulação realizada com sucesso.",
        typeof(SimularInvestimentoResponse))]
    [SwaggerResponseExample(
        StatusCodes.Status200OK,
        typeof(SimularInvestimentoResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetros inválidos ou inconsistentes.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro inesperado ao simular investimento.")]
    public async Task<ActionResult<SimularInvestimentoResponse>> SimularInvestimento(
        [FromBody] SimularInvestimentoRequest request)
    {
        try
        {
            var result = await _simulacoesService.SimularAsync(request);
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

    [HttpPost("simular-e-contratar-investimento")]
    [SwaggerOperation(
        Summary = "Simula e já contrata o investimento em uma única operação.",
        Description = "Realiza a simulação com base nos parâmetros informados, efetiva imediatamente a simulação " +
                      "como investimento real e atualiza o perfil de risco dinâmico do cliente."
    )]
    [SwaggerRequestExample(typeof(SimularInvestimentoRequest), typeof(SimularInvestimentoRequestExample))]
    [SwaggerResponse(StatusCodes.Status200OK,
        "Simulação realizada e investimento efetivado com sucesso.",
        typeof(SimularEContratarInvestimentoResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetros inválidos ou inconsistentes.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro inesperado ao simular e contratar investimento.")]
    public async Task<ActionResult<SimularEContratarInvestimentoResponse>> SimularEContratarInvestimento(
        [FromBody] SimularInvestimentoRequest request)
    {
        try
        {
            var result = await _simulacoesService.SimularEContratarAsync(request);
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
            return StatusCode(500, new { message = "Erro inesperado ao simular e contratar investimento." });
        }
    }

    [HttpGet("simulacoes")]
    [SwaggerOperation(
        Summary = "Lista o histórico de simulações realizadas.",
        Description = "Retorna todas as simulações armazenadas no sistema, com identificação de cliente, produto e valores."
    )]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Histórico de simulações retornado com sucesso.",
        typeof(IEnumerable<SimulacaoHistoricoResponse>))]
    [SwaggerResponseExample(
        StatusCodes.Status200OK,
        typeof(SimulacoesHistoricoExample))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    public async Task<ActionResult<IEnumerable<SimulacaoHistoricoResponse>>> GetSimulacoes()
    {
        var simulacoes = await _simulacoesService.ObterHistoricoAsync();
        return Ok(simulacoes);
    }

    [HttpGet("simulacoes/por-produto-dia")]
    [SwaggerOperation(
        Summary = "Retorna o resumo das simulações agrupadas por produto e data.",
        Description = "Fornece dados consolidados para análises e dashboards, agrupando simulações por produto e dia, " +
                      "incluindo quantidade de simulações e média do valor final."
    )]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Resumo de simulações retornado com sucesso.",
        typeof(IEnumerable<SimulacoesPorProdutoDiaResponse>))]
    [SwaggerResponseExample(
        StatusCodes.Status200OK,
        typeof(SimulacoesPorProdutoDiaExample))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    public async Task<ActionResult<IEnumerable<SimulacoesPorProdutoDiaResponse>>> GetSimulacoesPorProdutoDia()
    {
        var resumo = await _simulacoesService.ObterResumoPorProdutoDiaAsync();
        return Ok(resumo);
    }
}
