namespace ApiCaixaInvest.Application.Interfaces;

public interface ITokenStore
{
    Task StoreRefreshTokenAsync(string subject, string refreshToken, DateTime expiresAt);
    Task<bool> IsRefreshTokenValidAsync(string subject, string refreshToken);
    Task RevokeRefreshTokenAsync(string subject, string refreshToken);
}
