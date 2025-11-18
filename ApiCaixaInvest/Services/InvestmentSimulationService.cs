using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Requests;
using ApiCaixaInvest.Dtos.Responses;
using ApiCaixaInvest.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Services;

public class InvestmentSimulationService
{
    private readonly ApiCaixaInvestDbContext _db;

    public InvestmentSimulationService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request)
    {
        // 1) validações básicas
        if (request.Valor <= 0)
            throw new ArgumentException("O valor do investimento deve ser maior que zero.");

        if (request.PrazoMeses <= 0)
            throw new ArgumentException("O prazo em meses deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(request.TipoProduto))
            throw new ArgumentException("O tipo de produto é obrigatório.");

        // 2) buscar produtos compatíveis no banco
        var produtos = await _db.ProdutosInvestimento
            .Where(p => p.Tipo.ToLower() == request.TipoProduto.ToLower()
                        && p.PrazoMinimoMeses <= request.PrazoMeses)
            .ToListAsync();

        if (!produtos.Any())
            throw new InvalidOperationException("Nenhum produto compatível encontrado para os parâmetros informados.");

        // 3) por enquanto, regra simples: escolhe o de maior rentabilidade
        var produtoEscolhido = produtos
            .OrderByDescending(p => p.RentabilidadeAnual)
            .First();

        // 4) cálculo da simulação
        decimal anos = request.PrazoMeses / 12m;
        decimal valorFinal = request.Valor * (1 + produtoEscolhido.RentabilidadeAnual * anos);
        valorFinal = decimal.Round(valorFinal, 2);

        // 5) salvar simulação no banco
        var simulacao = new SimulacaoInvestimento
        {
            ClienteId = request.ClienteId,
            ProdutoInvestimentoId = produtoEscolhido.Id,
            ValorInvestido = request.Valor,
            ValorFinal = valorFinal,
            PrazoMeses = request.PrazoMeses,
            DataSimulacao = DateTime.Now
        };

        _db.Simulacoes.Add(simulacao);
        await _db.SaveChangesAsync();

        // 6) montar DTO de resposta
        var response = new SimularInvestimentoResponse
        {
            ProdutoValidado = new ProdutoResponse
            {
                Id = produtoEscolhido.Id,
                Nome = produtoEscolhido.Nome,
                Tipo = produtoEscolhido.Tipo,
                Rentabilidade = produtoEscolhido.RentabilidadeAnual,
                Risco = produtoEscolhido.Risco
            },
            ResultadoSimulacao = new ResultadoSimulacaoResponse
            {
                ValorFinal = valorFinal,
                RentabilidadeEfetiva = produtoEscolhido.RentabilidadeAnual,
                PrazoMeses = request.PrazoMeses
            },
            DataSimulacao = simulacao.DataSimulacao
        };

        return response;
    }
}
