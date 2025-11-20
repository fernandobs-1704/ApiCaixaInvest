using ApiCaixaInvest.Api.SwaggerExamples.Telemetria;
using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TelemetriaController : ControllerBase
{
    private readonly ITelemetriaQueryService _telemetriaQueryService;

    public TelemetriaController(ITelemetriaQueryService telemetriaQueryService)
    {
        _telemetriaQueryService = telemetriaQueryService;
    }

    [HttpGet("telemetria")]
    [SwaggerOperation(
        Summary = "Retorna o resumo de telemetria dos serviços da API.",
        Description = "Informa o volume de chamadas e o tempo médio de resposta por serviço " +
                      "em um período informado. Use os parâmetros de query 'inicio' e 'fim' " +
                      "no formato yyyy-MM-dd."
    )]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Resumo de telemetria retornado com sucesso.",
        typeof(TelemetriaResponse))]
    [SwaggerResponseExample(
        StatusCodes.Status200OK,
        typeof(TelemetriaResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Período inválido (data de fim menor que a de início).")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    public async Task<ActionResult<TelemetriaResponse>> GetTelemetria(
        [FromQuery] DateOnly inicio,
        [FromQuery] DateOnly fim)
    {
        if (fim < inicio)
        {
            return BadRequest(new { message = "A data de fim deve ser maior ou igual à data de início." });
        }

        var response = await _telemetriaQueryService.ObterResumoAsync(inicio, fim);
        return Ok(response);
    }
}
