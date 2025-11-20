using ApiCaixaInvest.Api.SwaggerExamples.Investimentos;
using ApiCaixaInvest.Application.Dtos.Requests.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class InvestimentosController : ControllerBase
{
    private readonly IInvestimentosService _investimentosService;
    private readonly IRiskProfileService _perfilRiscoService;

    public InvestimentosController(
        IInvestimentosService investimentosService,
        IRiskProfileService riskProfileService)
    {
        _investimentosService = investimentosService;
        _perfilRiscoService = riskProfileService;
    }

    [HttpGet("investimentos/{clienteId:int}")]
    [SwaggerOperation(
        Summary = "Retorna o histórico de investimentos realizados pelo cliente.",
        Description = "Lista os investimentos efetivados, que são utilizados no cálculo do perfil de risco dinâmico."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Histórico de investimentos retornado com sucesso.",
        typeof(IEnumerable<InvestimentoHistoricoResponse>))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(InvestimentoHistoricoResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetro clienteId inválido.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    public async Task<ActionResult<IEnumerable<InvestimentoHistoricoResponse>>> GetInvestimentos(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

        var investimentos = await _investimentosService.ObterHistoricoAsync(clienteId);
        return Ok(investimentos);
    }


    [HttpPost("investimentos/efetivar")]
    [SwaggerOperation(
      Summary = "Efetiva simulações de investimento.",
      Description = "Transforma uma ou mais simulações já realizadas em investimentos reais, que passam a influenciar o perfil de risco do cliente."
  )]
    [SwaggerRequestExample(typeof(EfetivarSimulacoesRequest), typeof(EfetivarSimulacoesRequestExample))]
    [SwaggerResponse(StatusCodes.Status200OK, "Simulações efetivadas com sucesso.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetros inválidos.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    public async Task<IActionResult> EfetivarSimulacoes([FromBody] EfetivarSimulacoesRequest request)
    {
        if (request.ClienteId <= 0)
            return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

        if (request.SimulacaoIds == null || !request.SimulacaoIds.Any())
            return BadRequest(new { mensagem = "Informe ao menos uma simulação para efetivar." });

        // Efetiva simulações
        await _investimentosService.EfetivarSimulacoesAsync(request.ClienteId, request.SimulacaoIds);

        // Recalcula perfil automaticamente
        var perfil = await _perfilRiscoService.CalcularPerfilAsync(request.ClienteId);

        // Retorno amigável 200 OK
        return Ok(new
        {
            sucesso = true,
            mensagem = "Simulações efetivadas com sucesso.",
            clienteId = request.ClienteId,
            simulacoesEfetivadas = request.SimulacaoIds,
            novoPerfilRisco = new
            {
                perfil = perfil.Perfil,
                pontuacao = perfil.Pontuacao
            }
        });
    }

}
