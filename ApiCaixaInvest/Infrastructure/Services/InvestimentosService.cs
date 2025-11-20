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

    public async Task<IReadOnlyList<InvestimentoHistoricoResponse>> ObterHistoricoClienteAsync(int clienteId)
    {
        var investimentos = await _db.InvestimentosHistorico
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

        return investimentos;
    }
}
