using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services;

public class InvestimentosService : IInvestimentosService
{
    private readonly ApiCaixaInvestDbContext _db;

    public InvestimentosService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoAsync(int clienteId)
    {
        var query = await _db.InvestimentosHistorico
            .Where(i => i.ClienteId == clienteId)
            .OrderByDescending(i => i.Data)
            .Select(i => new InvestimentoHistoricoResponse
            {
                Id = i.Id,
                Tipo = i.Tipo,
                Valor = i.Valor,
                Rentabilidade = i.Rentabilidade,
                Data = i.Data
            })
            .ToListAsync();

        return query;
    }

    public async Task EfetivarSimulacoesAsync(int clienteId, IEnumerable<int> simulacaoIds)
    {
        var idsLista = simulacaoIds?.Distinct().ToList() ?? new List<int>();
        if (!idsLista.Any())
            return;

        // Busca as simulações do cliente que ainda não foram efetivadas
        var simulacoes = await _db.Simulacoes
            .Include(s => s.ProdutoInvestimento)
            .Where(s => s.ClienteId == clienteId
                        && idsLista.Contains(s.Id)
                        && !s.Efetivada)
            .ToListAsync();

        if (!simulacoes.Any())
            return;

        foreach (var sim in simulacoes)
        {
            var produto = sim.ProdutoInvestimento;

            // Cria registro de investimento realizado
            var investimento = new Domain.Models.InvestimentoHistorico
            {
                ClienteId = sim.ClienteId,
                ProdutoInvestimentoId = sim.ProdutoInvestimentoId,
                Tipo = produto?.Tipo ?? "Desconhecido",
                Valor = sim.ValorInvestido,
                Rentabilidade = produto?.RentabilidadeAnual ?? 0m,
                Data = DateTime.Now
            };

            _db.InvestimentosHistorico.Add(investimento);

            // Marca simulação como efetivada para evitar duplicidade
            sim.Efetivada = true;
        }

        await _db.SaveChangesAsync();
    }
}
