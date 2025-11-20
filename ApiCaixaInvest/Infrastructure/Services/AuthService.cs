using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiCaixaInvest.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwtOptions;

    public AuthService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public Task<LoginResponse?> AutenticarAsync(LoginRequest request)
    {
        string perfil;
        if (request.Email.Equals("cliente@caixa.gov.br", StringComparison.OrdinalIgnoreCase) &&
            request.Senha == "123456")
        {
            perfil = "Cliente";
        }
        else if (request.Email.Equals("gestor@caixa.gov.br", StringComparison.OrdinalIgnoreCase) &&
                 request.Senha == "123456")
        {
            perfil = "Gestor";
        }
        else
        {
            // login inválido
            return Task.FromResult<LoginResponse?>(null);
        }

        var agora = DateTime.UtcNow;
        var expira = agora.AddMinutes(_jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Email),
            new Claim(JwtRegisteredClaimNames.Email, request.Email),
            new Claim(ClaimTypes.Name, request.Email),
            new Claim(ClaimTypes.Role, perfil),
            new Claim("perfil", perfil)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: agora,
            expires: expira,
            signingCredentials: credenciais);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponse
        {
            Token = tokenString,
            ExpiraEm = expira,
            Email = request.Email,
            Perfil = perfil
        };

        return Task.FromResult<LoginResponse?>(response);
    }
}
