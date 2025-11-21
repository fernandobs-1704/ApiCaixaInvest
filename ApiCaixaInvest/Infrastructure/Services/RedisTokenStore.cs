using ApiCaixaInvest.Application.Interfaces;
using StackExchange.Redis;

namespace ApiCaixaInvest.Infrastructure.Services;

public class RedisTokenStore : ITokenStore
{
    private readonly IConnectionMultiplexer _redis;

    public RedisTokenStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private IDatabase Db => _redis.GetDatabase();

    private static string BuildKey(string subject, string refreshToken)
        => $"refresh:{subject}:{refreshToken}";

    public async Task StoreRefreshTokenAsync(string subject, string refreshToken, DateTime expiresAt)
    {
        var key = BuildKey(subject, refreshToken);
        var ttl = expiresAt - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromMinutes(1);
        }

        await Db.StringSetAsync(key, "1", ttl);
    }

    public async Task<bool> IsRefreshTokenValidAsync(string subject, string refreshToken)
    {
        var key = BuildKey(subject, refreshToken);
        return await Db.KeyExistsAsync(key);
    }

    public async Task RevokeRefreshTokenAsync(string subject, string refreshToken)
    {
        var key = BuildKey(subject, refreshToken);
        await Db.KeyDeleteAsync(key);
    }
}
