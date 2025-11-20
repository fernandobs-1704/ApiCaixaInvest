using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services;

public class SimulacoesConsultaService : ISimulacoesConsultaService
{
    private readonly ApiCaixaInvestDbContext _db;

    public SimulacoesConsultaService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SimulacaoHistoricoResponse>> ObterHistoricoAsync()
    {
        var simulacoes = await _db.Simulacoes
            .Include(s => s.ProdutoInvestimento)
            .OrderByDescending(s => s.DataSimulacao)
            .Select(s => new SimulacaoHistoricoResponse
            {
                Id = s.Id,
                ClienteId = s.ClienteId,
                Produto = s.ProdutoInvestimento != null ? s.ProdutoInvestimento.Nome : string.Empty,
                ValorInvestido = s.ValorInvestido,
                ValorFinal = s.ValorFinal,
                PrazoMeses = s.PrazoMeses,
                DataSimulacao = s.DataSimulacao
            })
            .ToListAsync();

        return simulacoes;
    }

    public async Task<IReadOnlyList<SimulacoesPorProdutoDiaResponse>> ObterResumoPorProdutoDiaAsync()
    {
        var query = await _db.Simulacoes
            .Include(s => s.ProdutoInvestimento)
            .GroupBy(s => new
            {
                Produto = s.ProdutoInvestimento != null ? s.ProdutoInvestimento.Nome : string.Empty,
                Data = s.DataSimulacao.Date
            })
            .Select(g => new SimulacoesPorProdutoDiaResponse
            {
                Produto = g.Key.Produto,
                Data = g.Key.Data,
                QuantidadeSimulacoes = g.Count(),
                MediaValorFinal = g.Average(x => x.ValorFinal)
            })
            .OrderBy(r => r.Produto)
            .ThenBy(r => r.Data)
            .ToListAsync();

        return query;
    }
}
