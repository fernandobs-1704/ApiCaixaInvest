using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Retorna o resumo de telemetria dos serviços da API em um período.
    /// </summary>
    /// <remarks>
    /// Informe as datas de início e fim no formato yyyy-MM-dd via query string.
    /// Exemplo: GET /api/telemetria?inicio=2025-10-01&amp;fim=2025-10-31
    /// </remarks>
    [HttpGet("telemetria")]
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
