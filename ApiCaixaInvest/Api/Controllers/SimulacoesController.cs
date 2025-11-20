using ApiCaixaInvest.Api.SwaggerExamples.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Infrastructure.Services;
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
    private readonly IInvestmentSimulationService _simulationService;
    private readonly ISimulacoesConsultaService _simulacoesConsultaService;
    private readonly IInvestimentosService _investimentosService;
    private readonly IRiskProfileService _perfilRiscoService;

    public SimulacoesController(
          IInvestmentSimulationService simulationService,
          ISimulacoesConsultaService simulacoesConsultaService,
          IInvestimentosService investimentosService,
          IRiskProfileService perfilRiscoService)
    {
        _simulationService = simulationService;
        _simulacoesConsultaService = simulacoesConsultaService;
        _investimentosService = investimentosService;
        _perfilRiscoService = perfilRiscoService;
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

    [HttpPost("simular-e-contratar-investimento")]
    [SwaggerOperation(
        Summary = "Simula e já contrata o investimento em uma única operação.",
        Description = "Realiza a simulação com base nos parâmetros informados, efetiva imediatamente a simulação " +
                      "como investimento real e atualiza o perfil de risco dinâmico do cliente."
    )]
    [SwaggerRequestExample(typeof(SimularInvestimentoRequest), typeof(SimularInvestimentoRequestExample))]
    [SwaggerResponse(StatusCodes.Status200OK, "Simulação realizada e investimento efetivado com sucesso.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetros inválidos ou inconsistentes.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro inesperado ao simular e contratar investimento.")]
    public async Task<ActionResult<object>> SimularEContratarInvestimento(
        [FromBody] SimularInvestimentoRequest request)
    {
        try
        {
            // 1) Simula (e grava a simulação)
            var simulacao = await _simulationService.SimularAsync(request);

            // 2) Efetiva apenas essa simulação recém-criada
            await _investimentosService.EfetivarSimulacoesAsync(
                request.ClienteId,
                new[] { simulacao.SimulacaoId });

            // 3) Busca o investimento mais recente do cliente (provavelmente o que acabamos de criar)
            var historico = await _investimentosService.ObterHistoricoAsync(request.ClienteId);
            var investimento = historico
                .OrderByDescending(h => h.Data)
                .FirstOrDefault();

            // 4) Recalcula perfil de risco após a contratação
            var perfil = await _perfilRiscoService.CalcularPerfilAsync(request.ClienteId);

            return Ok(new
            {
                sucesso = true,
                mensagem = "Simulação realizada e investimento efetivado com sucesso.",
                clienteId = request.ClienteId,
                simulacao,
                investimento,
                perfilRisco = new
                {
                    perfil = perfil.Perfil,
                    perfilTipo = perfil.PerfilTipo.ToString(),
                    pontuacao = perfil.Pontuacao,
                    descricao = perfil.Descricao,
                    ultimaAtualizacao = perfil.UltimaAtualizacao
                }
            });
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
        var simulacoes = await _simulacoesConsultaService.ObterHistoricoAsync();
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
        var resumo = await _simulacoesConsultaService.ObterResumoPorProdutoDiaAsync();
        return Ok(resumo);
    }
}
