using System.Text.Json;
using ApiCaixaInvest.Application.Dtos.Responses.Produtos;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ApiCaixaInvest.Infrastructure.Services;

public class ProdutosService : IProdutosService
{
    private readonly ApiCaixaInvestDbContext _db;
    private readonly IDatabase? _cache;

    private const string CacheKeyPrefix = "produtos:recomendados:";

    /// <summary>
    /// Construtor principal.
    /// Em produção, o container de DI injeta o IConnectionMultiplexer (Redis).
    /// Em testes unitários, pode ser chamado apenas com o DbContext, sem Redis.
    /// </summary>
    public ProdutosService(ApiCaixaInvestDbContext db, IConnectionMultiplexer? redis = null)
    {
        _db = db;
        _cache = redis?.GetDatabase();
    }

    public async Task<IReadOnlyList<ProdutoRecomendadoResponse>> ObterProdutosRecomendadosAsync(string perfil)
    {
        if (string.IsNullOrWhiteSpace(perfil))
            throw new ArgumentException("O perfil é obrigatório.", nameof(perfil));

        var perfilNormalizado = perfil.Trim().ToLowerInvariant();
        var cacheKey = $"{CacheKeyPrefix}{perfilNormalizado}";

        List<ProdutoRecomendadoResponse>? fromCache = null;

        // 1) Tenta ler do Redis, se estiver configurado
        if (_cache != null)
        {
            try
            {
                var cached = await _cache.StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
                    fromCache = JsonSerializer.Deserialize<List<ProdutoRecomendadoResponse>>(cached!)
                                ?? new List<ProdutoRecomendadoResponse>();
                }
            }
            catch
            {
                // Em testes ou se o Redis estiver indisponível, simplesmente ignora o cache
                // e segue fluxo normal de banco.
            }
        }

        if (fromCache != null)
            return fromCache;

        // 2) Lógica original (banco de dados)
        string[] riscosPermitidos = perfilNormalizado switch
        {
            "conservador" => new[] { "Baixo" },
            "moderado" => new[] { "Baixo", "Médio", "Medio" },
            "agressivo" => new[] { "Baixo", "Médio", "Medio", "Alto" },
            _ => Array.Empty<string>()
        };

        if (!riscosPermitidos.Any())
            throw new InvalidOperationException("Perfil inválido. Use Conservador, Moderado ou Agressivo.");

        var produtosDb = await _db.ProdutosInvestimento
            .Where(p => riscosPermitidos.Contains(p.Risco))
            .ToListAsync();

        var produtos = produtosDb
            .OrderByDescending(p => p.RentabilidadeAnual)
            .Select(p => new ProdutoRecomendadoResponse
            {
                Id = p.Id,
                Nome = p.Nome,
                Tipo = p.Tipo,
                Rentabilidade = p.RentabilidadeAnual,
                Risco = p.Risco
            })
            .ToList();

        // 3) Grava no Redis, se disponível
        if (_cache != null && produtos.Any())
        {
            try
            {
                var json = JsonSerializer.Serialize(produtos);
                await _cache.StringSetAsync(
                    cacheKey,
                    json,
                    TimeSpan.FromMinutes(10)); // TTL 10 min
            }
            catch
            {
                // Se o Redis der erro, não derruba a API nem o teste.
            }
        }

        return produtos;
    }
}
