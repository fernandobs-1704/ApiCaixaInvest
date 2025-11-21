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
    private readonly ITokenStore _tokenStore;

    public AuthService(
        IOptions<JwtOptions> jwtOptions,
        ITokenStore tokenStore)
    {
        _jwtOptions = jwtOptions.Value;
        _tokenStore = tokenStore;
    }

    public async Task<LoginResponse?> AutenticarAsync(LoginRequest request)
    {
        string perfil;
        if (request.Email.Equals("caixaverso@caixa.gov.br", StringComparison.OrdinalIgnoreCase) &&
            request.Senha == "Caixaverso@2025")
        {
            perfil = "Usuario";
        }
        else
        {
            return null;
        }

        return await GerarTokensAsync(request.Email, perfil);
    }

    public async Task<LoginResponse?> RenovarTokenAsync(RefreshTokenRequest request)
    {
        // valida refresh token no Redis
        var valido = await _tokenStore.IsRefreshTokenValidAsync(request.Email, request.RefreshToken);
        if (!valido)
            return null;

        // revoga o antigo (token rotation)
        await _tokenStore.RevokeRefreshTokenAsync(request.Email, request.RefreshToken);

        // aqui no futuro você pode validar usuário num banco; por enquanto,
        // se chegou aqui, assume o mesmo perfil da autenticação inicial
        var perfil = "Usuario";

        return await GerarTokensAsync(request.Email, perfil);
    }

    private async Task<LoginResponse> GerarTokensAsync(string email, string perfil)
    {
        var agora = DateTime.UtcNow;
        var expira = agora.AddMinutes(_jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, email),
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

        // refresh token: 7 dias
        var refreshToken = Guid.NewGuid().ToString("N");
        var refreshExpira = agora.AddDays(7);

        await _tokenStore.StoreRefreshTokenAsync(email, refreshToken, refreshExpira);

        var response = new LoginResponse
        {
            Token = tokenString,
            ExpiraEm = expira,
            Email = email,
            Perfil = perfil,
            RefreshToken = refreshToken,
            RefreshTokenExpiraEm = refreshExpira
        };

        return response;
    }
}
