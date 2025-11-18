using ApiCaixaInvest.Dtos.Requests;
using ApiCaixaInvest.Dtos.Responses;
using ApiCaixaInvest.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class SimulacoesController : ControllerBase
{
    private readonly InvestmentSimulationService _simulationService;

    public SimulacoesController(InvestmentSimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    /// <summary>
    /// Executa a simulação de um produto de investimento.
    /// </summary>
    /// <remarks>
    /// <para>Valida os dados informados pelo cliente.</para>
    /// <para>Seleciona o produto de investimento compatível.</para>
    /// <para>Calcula o valor final com base na rentabilidade anual.</para>
    /// <para>Persiste a simulação no banco SQLite.</para>
    /// <para>Retorna o resumo completo da simulação.</para>
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
        catch (Exception ex)
        {
            // Em produção: logar isso. Aqui só genérico pro cliente.
            return StatusCode(500, new { message = "Erro inesperado ao simular investimento.", detail = ex.Message });
        }
    }
}
