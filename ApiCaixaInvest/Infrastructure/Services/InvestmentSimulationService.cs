using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services;

public class InvestmentSimulationService : IInvestmentSimulationService
{
    private readonly ApiCaixaInvestDbContext _db;
    private readonly IClienteService _clienteService;

    public InvestmentSimulationService(ApiCaixaInvestDbContext db, IClienteService clienteService)
    {
        _db = db;
        _clienteService = clienteService;
    }

    public async Task<SimularInvestimentoResponse> SimularAsync(SimularInvestimentoRequest request)
    {
        // 1) Validação inicial
        ValidarRequestSimulacao(request);

        // 2) Garante que o cliente exista (cria se não existir)
        await _clienteService.GarantirClienteAsync(request.ClienteId);

        // 3) Buscar produtos compatíveis no banco
        var tipoNormalizado = request.TipoProduto.Trim().ToLowerInvariant();

        var produtos = await _db.ProdutosInvestimento
            .Where(p =>
                p.Tipo.ToLower() == tipoNormalizado &&
                p.PrazoMinimoMeses <= request.PrazoMeses)
            .ToListAsync();

        if (!produtos.Any())
            throw new InvalidOperationException("Nenhum produto compatível encontrado para os parâmetros informados.");

        // 4) Regra simples: escolher produto com maior rentabilidade
        var produtoEscolhido = produtos
            .OrderByDescending(p => p.RentabilidadeAnual)
            .First();

        // 5) Cálculo da simulação
        decimal anos = request.PrazoMeses / 12m;
        decimal valorFinal = decimal.Round(
            request.Valor * (1 + produtoEscolhido.RentabilidadeAnual * anos),
            2
        );

        // 6) Persistir simulação
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

        // 7) Retorno estruturado
        return new SimularInvestimentoResponse
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
    }

    private static void ValidarRequestSimulacao(SimularInvestimentoRequest request)
    {
        if (request is null)
            throw new ArgumentException("A requisição não pode ser nula.");

        if (request.ClienteId <= 0)
            throw new ArgumentException("O identificador do cliente deve ser maior que zero.");

        if (request.Valor <= 0)
            throw new ArgumentException("O valor do investimento deve ser maior que zero.");

        if (request.PrazoMeses <= 0)
            throw new ArgumentException("O prazo em meses deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(request.TipoProduto))
            throw new ArgumentException("O tipo de produto é obrigatório.");
    }
}
