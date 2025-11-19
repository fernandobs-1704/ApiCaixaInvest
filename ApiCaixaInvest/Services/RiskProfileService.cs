using ApiCaixaInvest.Data;
using ApiCaixaInvest.Dtos.Responses;
using ApiCaixaInvest.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Services;

/// <summary>
/// Serviço responsável por calcular e persistir o perfil de risco do cliente.
/// </summary>
public class RiskProfileService
{
    private readonly ApiCaixaInvestDbContext _db;

    public RiskProfileService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId)
    {
        // Carrega o histórico de investimentos do cliente
        var historico = await _db.InvestimentosHistorico
            .Where(h => h.ClienteId == clienteId)
            .ToListAsync();

        if (!historico.Any())
        {
            // Sem histórico: assume conservador por padrão
            return await SalvarERetornarAsync(clienteId, "Conservador", 20,
                "Cliente sem histórico de investimentos; perfil definido como conservador por padrão.");
        }

        // Total investido (para ponderar risco pelo valor)
        var totalInvestido = historico.Sum(h => h.Valor);
        if (totalInvestido <= 0)
        {
            return await SalvarERetornarAsync(clienteId, "Conservador", 20,
                "Total investido igual ou inferior a zero; perfil definido como conservador.");
        }

        // Classificação simples de risco por tipo (poderia ser configurável)
        decimal valorRendaFixa = historico
            .Where(h => EhRendaFixa(h.Tipo))
            .Sum(h => h.Valor);

        decimal valorRiscoAlto = historico
            .Where(h => EhAltoRisco(h.Tipo))
            .Sum(h => h.Valor);

        // Pondera pelo percentual em produtos de maior risco
        var percAlta = valorRiscoAlto / totalInvestido;
        percAlta = Math.Clamp(percAlta, 0m, 1m);

        // Regra simples: quanto maior exposição ao risco alto, maior a pontuação
        // base 30 + até 70 pontos proporcionais à exposição em risco alto
        var pontuacao = (int)Math.Round(30m + percAlta * 70m, 0);

        string perfil;
        if (pontuacao < 40)
            perfil = "Conservador";
        else if (pontuacao <= 70)
            perfil = "Moderado";
        else
            perfil = "Agressivo";

        string explicacao = MontarExplicacao(perfil, pontuacao, percAlta, totalInvestido);

        return await SalvarERetornarAsync(clienteId, perfil, pontuacao, explicacao);
    }

    private static bool EhRendaFixa(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return false;

        tipo = tipo.ToLowerInvariant();

        return tipo.Contains("cdb")
            || tipo.Contains("lci")
            || tipo.Contains("lca")
            || tipo.Contains("tesouro")
            || tipo.Contains("renda fixa");
    }

    private static bool EhAltoRisco(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return false;

        tipo = tipo.ToLowerInvariant();

        return tipo.Contains("ações")
            || tipo.Contains("acao")
            || tipo.Contains("multimercado")
            || tipo.Contains("cripto")
            || tipo.Contains("derivativo");
    }

    private string MontarExplicacao(string perfil, int pontuacao, decimal percAlta, decimal totalInvestido)
    {
        var percAltaFmt = Math.Round(percAlta * 100m, 2);

        return perfil switch
        {
            "Conservador" =>
                $"Perfil {perfil} (pontuação {pontuacao}). Baixa exposição a ativos de maior risco e preferência por produtos de renda fixa. Exposição em risco alto: {percAltaFmt}% do total investido.",
            "Moderado" =>
                $"Perfil {perfil} (pontuação {pontuacao}). Combinação equilibrada entre renda fixa e ativos de maior risco. Exposição em risco alto: {percAltaFmt}% do total investido.",
            "Agressivo" =>
                $"Perfil {perfil} (pontuação {pontuacao}). Alta exposição a ativos de maior risco e maior tolerância a volatilidade. Exposição em risco alto: {percAltaFmt}% do total investido.",
            _ =>
                $"Perfil {perfil} (pontuação {pontuacao}). Total investido analisado: {totalInvestido:C2}."
        };
    }

    private async Task<PerfilRiscoResponse> SalvarERetornarAsync(
        int clienteId,
        string perfil,
        int pontuacao,
        string explicacao)
    {
        var dataAtual = DateTime.Now;

        // Verifica se já existe um registro de perfil para o cliente
        var existente = await _db.PerfisClientes
            .FirstOrDefaultAsync(p => p.ClienteId == clienteId);

        if (existente == null)
        {
            existente = new PerfilCliente
            {
                ClienteId = clienteId,
                Perfil = perfil,
                Pontuacao = pontuacao,
                UltimaAtualizacao = dataAtual
            };
            _db.PerfisClientes.Add(existente);
        }
        else
        {
            existente.Perfil = perfil;
            existente.Pontuacao = pontuacao;
            existente.UltimaAtualizacao = dataAtual;
        }

        await _db.SaveChangesAsync();

        return new PerfilRiscoResponse
        {
            ClienteId = clienteId,
            Perfil = perfil,
            Pontuacao = pontuacao,
            UltimaAtualizacao = dataAtual,
            Explicacao = explicacao
        };
    }
}
