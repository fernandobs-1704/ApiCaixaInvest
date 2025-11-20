using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Autentica o usuário e retorna um token JWT.
    /// </summary>
    /// <remarks>
    /// Use os usuários de exemplo:
    /// - usuario: cliente@caixa.gov.br / senha: 123456
    /// - usuario: gestor@caixa.gov.br /  senha: 123456
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.AutenticarAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(result);
    }

    /// <summary>
    /// Endpoint de teste para validar se o token está funcionando.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            Usuario = User.Identity?.Name,
            Perfil = User.Claims.FirstOrDefault(c => c.Type == "perfil")?.Value
        });
    }
}
