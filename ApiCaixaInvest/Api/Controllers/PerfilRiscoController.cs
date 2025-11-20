using ApiCaixaInvest.Api.SwaggerExamples.PerfilRisco;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class PerfilRiscoController : ControllerBase
{
    private readonly IRiskProfileService _riskProfileService;

    public PerfilRiscoController(IRiskProfileService riskProfileService)
    {
        _riskProfileService = riskProfileService;
    }

    [HttpGet("perfil-risco/{clienteId:int}")]
    [SwaggerOperation(
        Summary = "Calcula e retorna o perfil de risco do cliente.",
        Description = "Analisa o comportamento de investimentos do cliente (volume, frequência e nível de risco dos produtos). " +
                      "Classifica automaticamente em Conservador, Moderado ou Agressivo."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Perfil de risco calculado com sucesso.", typeof(PerfilRiscoResponse))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PerfilRiscoResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetro clienteId inválido.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro inesperado ao calcular o perfil de risco.")]
    public async Task<ActionResult<PerfilRiscoResponse>> ObterPerfilRisco(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

        try
        {
            var resultado = await _riskProfileService.CalcularPerfilAsync(clienteId);
            return Ok(resultado);
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensagem = "Erro ao calcular o perfil de risco do cliente." });
        }
    }
}
