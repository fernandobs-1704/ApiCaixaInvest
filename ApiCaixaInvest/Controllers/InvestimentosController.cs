using ApiCaixaInvest.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api")]
public class InvestimentosController : ControllerBase
{
    private readonly IInvestimentosService _investimentosService;

    public InvestimentosController(IInvestimentosService investimentosService)
    {
        _investimentosService = investimentosService;
    }

    /// <summary>
    /// Retorna o histórico de investimentos do cliente.
    /// </summary>
    [HttpGet("investimentos/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<InvestimentoHistoricoResponse>>> GetInvestimentos(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { message = "O identificador do cliente deve ser maior que zero." });

        var investimentos = await _investimentosService.ObterHistoricoClienteAsync(clienteId);
        return Ok(investimentos);
    }
}
