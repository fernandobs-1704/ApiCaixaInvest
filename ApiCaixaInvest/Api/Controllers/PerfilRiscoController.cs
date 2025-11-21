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
        Summary = "Calcula e retorna o perfil de risco do cliente (versão simples).",
        Description = "Retorna o resultado exatamente conforme o enunciado: clienteId, perfil, pontuação e descrição simples."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Perfil de risco calculado com sucesso.", typeof(PerfilRiscoBasicoResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetro clienteId inválido.")]
    public async Task<ActionResult<PerfilRiscoBasicoResponse>> ObterPerfilRiscoBasico(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

        var completo = await _riskProfileService.CalcularPerfilAsync(clienteId);

        var basico = new PerfilRiscoBasicoResponse
        {
            ClienteId = completo.ClienteId,
            Perfil = completo.Perfil,
            Pontuacao = completo.Pontuacao,
            Descricao = completo.Descricao
        };

        return Ok(basico);
    }

    [HttpGet("perfil-risco/detalhado/{clienteId:int}")]
    [SwaggerOperation(
     Summary = "Retorna o perfil de risco detalhado do cliente.",
     Description = "Inclui análise estendida com liquidez, frequência, composição de carteira, tendência Markoviana e próximo perfil provável."
 )]
    [SwaggerResponse(StatusCodes.Status200OK, "Perfil de risco completo retornado com sucesso.", typeof(PerfilRiscoResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetro clienteId inválido.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro interno ao calcular o perfil de risco.")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(PerfilRiscoResponseExample))]
    public async Task<ActionResult<PerfilRiscoResponse>> ObterPerfilRiscoDetalhado(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

        try
        {
            var resultado = await _riskProfileService.CalcularPerfilAsync(clienteId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            // Aqui você pode logar a exceção com ILogger, se tiver injetado.
            // _logger.LogError(ex, "Erro ao calcular perfil de risco para o cliente {ClienteId}", clienteId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    mensagem = "Ocorreu um erro ao calcular o perfil de risco.",
                    detalhe = ex.Message
                });
        }
    }


    [HttpGet("perfil-risco-ia/{clienteId:int}")]
    [SwaggerOperation(
    Summary = "Gera uma explicação didática do perfil de risco, em linguagem natural.",
    Description = "Retorna um texto estruturado com resumo, explicação do comportamento, sugestões estratégicas, ações recomendadas e alertas importantes, usando a tendência Markoviana de perfis."
)]
    [SwaggerResponse(
    StatusCodes.Status200OK,
    "Explicação em linguagem natural gerada com sucesso.",
    typeof(PerfilRiscoIaResponse))]
    [SwaggerResponseExample(
    StatusCodes.Status200OK,
    typeof(PerfilRiscoIaResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetro clienteId inválido.")]
    public async Task<ActionResult<PerfilRiscoIaResponse>> ObterPerfilRiscoIa(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

        var resposta = await _riskProfileService.GerarExplicacaoIaAsync(clienteId);
        return Ok(resposta);
    }

}
