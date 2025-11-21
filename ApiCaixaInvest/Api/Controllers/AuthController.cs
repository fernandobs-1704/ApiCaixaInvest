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
        Description = "Use as seguintes credenciais: " +
                      "*** caixaverso@caixa.gov.br / Caixaverso@2025 ***"
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

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Renova o token de acesso usando um refresh token válido.",
        Description = "Envia o e-mail do usuário e o refresh token obtido no login."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Token renovado com sucesso.", typeof(LoginResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Requisição inválida.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Refresh token inválido ou expirado.")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RenovarTokenAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Refresh token inválido ou expirado." });

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Teste para validar autenticação.",
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
