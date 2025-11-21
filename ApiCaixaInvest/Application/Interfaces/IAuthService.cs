using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;

namespace ApiCaixaInvest.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> AutenticarAsync(LoginRequest request);
    Task<LoginResponse?> RenovarTokenAsync(RefreshTokenRequest request);
}
