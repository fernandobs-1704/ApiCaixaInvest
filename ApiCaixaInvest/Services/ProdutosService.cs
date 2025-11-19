using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Responses.Produtos;
using ApiCaixaInvest.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Services;

public class ProdutosService : IProdutosService
{
    private readonly ApiCaixaInvestDbContext _db;

    public ProdutosService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProdutoRecomendadoResponse>> ObterProdutosRecomendadosAsync(string perfil)
    {
        if (string.IsNullOrWhiteSpace(perfil))
            throw new ArgumentException("O perfil é obrigatório.", nameof(perfil));

        var perfilNormalizado = perfil.Trim().ToLowerInvariant();

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

        return produtos;
    }
}
