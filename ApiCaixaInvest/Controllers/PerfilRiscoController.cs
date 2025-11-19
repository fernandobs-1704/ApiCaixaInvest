using ApiCaixaInvest.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class PerfilRiscoController : ControllerBase
{
    private readonly RiskProfileService _riskProfileService;

    public PerfilRiscoController(RiskProfileService riskProfileService)
    {
        _riskProfileService = riskProfileService;
    }

    /// <summary>
    /// Calcula e retorna o perfil de risco do cliente.
    /// </summary>
    /// <remarks>
    /// Analisa o histórico de investimentos e classifica o cliente como conservador, moderado ou agressivo.
    /// </remarks>
    [HttpGet("perfil-risco/{clienteId:int}")]
    public async Task<ActionResult<PerfilRiscoResponse>> ObterPerfilRisco(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { message = "O identificador do cliente deve ser maior que zero." });

        try
        {
            var resultado = await _riskProfileService.CalcularPerfilAsync(clienteId);
            return Ok(resultado);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro ao calcular o perfil de risco do cliente." });
        }
    }
}
