using ApiCaixaInvest.Api.SwaggerExamples.Auth;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Autentica o usuário e retorna um token JWT.",
        Description = "Use os usuários de exemplo para testes: " +
                      "**cliente@caixa.gov.br / 123456** ou **gestor@caixa.gov.br / 123456**."
    )]
    [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
    [SwaggerResponse(StatusCodes.Status200OK, "Autenticação realizada com sucesso.", typeof(LoginResponse))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseExample))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Requisição inválida.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Credenciais inválidas.")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.AutenticarAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(result);
    }


    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Retorna informações básicas do usuário autenticado.",
        Description = "Endpoint de teste para validar se o token JWT está funcionando corretamente."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Usuário autenticado retornado com sucesso.")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MeResponseExample))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Token JWT ausente ou inválido.")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            Usuario = User.Identity?.Name,
            Perfil = User.Claims.FirstOrDefault(c => c.Type == "perfil")?.Value
        });
    }
}
