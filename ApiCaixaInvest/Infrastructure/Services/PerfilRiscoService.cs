using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Enum;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services;

/// <summary>
/// Serviço responsável por calcular e persistir o perfil de risco do cliente,
/// considerando volume, frequência, liquidez e rentabilidade, conforme enunciado.
/// </summary>
public class PerfilRiscoService : IRiskProfileService
{
    private readonly ApiCaixaInvestDbContext _db;

    public PerfilRiscoService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    public async Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId)
    {
        // 1) Carrega o histórico de investimentos do cliente
        var historico = await _db.InvestimentosHistorico
            .Where(h => h.ClienteId == clienteId)
            .ToListAsync();

        // Sem histórico: assume conservador por padrão
        if (!historico.Any())
        {
            const int pontuacaoDefault = 20;
            const string descricaoDefault =
                "Cliente sem histórico de investimentos; perfil definido como conservador por padrão.";

            return await SalvarERetornarAsync(
                clienteId,
                PerfilRiscoTipoEnum.Conservador,
                pontuacaoDefault,
                descricaoDefault);
        }

        // 2) Volume total investido
        decimal totalInvestido = historico.Sum(h => h.Valor);
        if (totalInvestido <= 0)
        {
            const int pontuacaoDefault = 20;
            const string descricaoDefault =
                "Total investido igual ou inferior a zero; perfil definido como conservador por padrão.";

            return await SalvarERetornarAsync(
                clienteId,
                PerfilRiscoTipoEnum.Conservador,
                pontuacaoDefault,
                descricaoDefault);
        }

        // 3) Frequência de movimentações (últimos 12 meses)
        var agora = DateTime.Now;
        var corte12Meses = agora.AddMonths(-12);
        int frequencia12Meses = historico
            .Count(h => h.Data >= corte12Meses);

        // 4) Composição de risco da carteira (alto risco vs renda fixa)
        decimal valorRendaFixa = historico
            .Where(h => EhRendaFixa(h.Tipo))
            .Sum(h => h.Valor);

        decimal valorRiscoAlto = historico
            .Where(h => EhAltoRisco(h.Tipo))
            .Sum(h => h.Valor);

        decimal percAlta = valorRiscoAlto / totalInvestido;
        percAlta = Math.Clamp(percAlta, 0m, 1m);

        // 5) Preferência por liquidez vs rentabilidade via produtos de investimento
        var produtos = await _db.ProdutosInvestimento.ToListAsync();

        // Faz um "join" aproximado por Tipo (case-insensitive)
        var joinHistProd = (from h in historico
                            join p in produtos
                                on h.Tipo.ToLower() equals p.Tipo.ToLower()
                                into gj
                            from p in gj.DefaultIfEmpty()
                            select new { Historico = h, Produto = p })
                            .ToList();

        // Média de liquidez dos produtos efetivamente usados (fallback para 30 dias)
        decimal mediaLiquidezDias = joinHistProd
            .Where(x => x.Produto != null)
            .Select(x => (decimal)x.Produto!.LiquidezDias)
            .DefaultIfEmpty(30m)
            .Average();

        // Média de rentabilidade anual dos produtos (fallback 0.10 = 10% a.a.)
        decimal mediaRentabilidade = joinHistProd
            .Where(x => x.Produto != null)
            .Select(x => x.Produto!.RentabilidadeAnual)
            .DefaultIfEmpty(0.10m)
            .Average();

        // 6) Cálculo dos "scores" individuais (cada eixo)
        int scoreVolume = CalcularScoreVolume(totalInvestido);
        int scoreFrequencia = CalcularScoreFrequencia(frequencia12Meses);
        int scoreLiquidez = CalcularScoreLiquidez(mediaLiquidezDias);
        int scoreRentabilidade = CalcularScoreRentabilidade(mediaRentabilidade);
        int scoreRiscoCarteira = CalcularScoreRiscoCarteira(percAlta);

        // 7) Pontuação final (soma dos eixos)
        int pontuacaoFinal =
            scoreVolume +
            scoreFrequencia +
            scoreLiquidez +
            scoreRentabilidade +
            scoreRiscoCarteira;

        // 8) Classificação do perfil com base na pontuação global
        PerfilRiscoTipoEnum perfilTipo = pontuacaoFinal switch
        {
            <= 80 => PerfilRiscoTipoEnum.Conservador,
            <= 140 => PerfilRiscoTipoEnum.Moderado,
            _ => PerfilRiscoTipoEnum.Agressivo
        };

        string descricao = MontarDescricao(
            perfilTipo,
            pontuacaoFinal,
            totalInvestido,
            frequencia12Meses,
            mediaLiquidezDias,
            mediaRentabilidade,
            percAlta);

        return await SalvarERetornarAsync(clienteId, perfilTipo, pontuacaoFinal, descricao);
    }

    #region Cálculo de scores

    private static int CalcularScoreVolume(decimal totalInvestido)
    {
        // Escala simples: quanto maior o volume, maior a capacidade/tolerância de risco
        return totalInvestido switch
        {
            < 5_000m => 10,
            < 20_000m => 20,
            < 100_000m => 30,
            _ => 40
        };
    }

    private static int CalcularScoreFrequencia(int freq12Meses)
    {
        // Quanto mais movimenta, mais propenso a buscar oportunidades (mais agressivo)
        return freq12Meses switch
        {
            0 => 10, // quase não mexe
            1 => 20,
            <= 6 => 30,
            _ => 40  // movimentação alta
        };
    }

    private static int CalcularScoreLiquidez(decimal mediaLiquidezDias)
    {
        // Poucos dias de liquidez => forte preferência por liquidez (tende a conservador)
        return mediaLiquidezDias switch
        {
            <= 30m => 40, // alta liquidez
            <= 90m => 25,
            _ => 10  // aceita baixa liquidez => mais tolerante a risco
        };
    }

    private static int CalcularScoreRentabilidade(decimal mediaRentabilidade)
    {
        // Maior rentabilidade média sugere busca por retorno maior (mais agressivo)
        return mediaRentabilidade switch
        {
            < 0.08m => 10,
            < 0.12m => 20,
            < 0.20m => 30,
            _ => 40
        };
    }

    private static int CalcularScoreRiscoCarteira(decimal percAlta)
    {
        // Proporção de ativos de alto risco na carteira, mapeada para 0 a 40 pontos
        return (int)Math.Round(percAlta * 40m, 0);
    }

    #endregion

    #region Helpers de classificação de produtos por tipo (alto risco x renda fixa)

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
            || tipo.Contains("derivativo")
            || tipo.Contains("fundo"); // fundos em geral tendem a ter mais risco
    }

    #endregion

    #region Montagem da descrição (descricao) para o response

    private string MontarDescricao(
        PerfilRiscoTipoEnum perfil,
        int pontuacao,
        decimal totalInvestido,
        int frequencia12Meses,
        decimal mediaLiquidezDias,
        decimal mediaRentabilidade,
        decimal percAltaRisco)
    {
        var percAltaFmt = Math.Round(percAltaRisco * 100m, 2);
        var liquidezFmt = Math.Round(mediaLiquidezDias, 1);
        var rentabFmt = Math.Round(mediaRentabilidade * 100m, 2);

        var baseDescricao = perfil switch
        {
            PerfilRiscoTipoEnum.Conservador =>
                "Perfil conservador: foco em segurança, preferência por liquidez e baixa exposição a ativos de maior risco.",
            PerfilRiscoTipoEnum.Moderado =>
                "Perfil moderado: equilíbrio entre segurança e rentabilidade, com alguma exposição a ativos de maior risco.",
            PerfilRiscoTipoEnum.Agressivo =>
                "Perfil agressivo: maior tolerância a risco, buscando retornos mais elevados e aceitando maior volatilidade.",
            _ =>
                "Perfil de risco calculado a partir do comportamento de investimentos do cliente."
        };

        var detalhes =
            $" Pontuação {pontuacao}. Total investido analisado: {totalInvestido:C2}. " +
            $"Movimentações nos últimos 12 meses: {frequencia12Meses}. " +
            $"Liquidez média dos produtos: ~{liquidezFmt} dias. " +
            $"Rentabilidade média anual: {rentabFmt}%. " +
            $"Exposição a ativos de maior risco: {percAltaFmt}% do total investido.";

        return baseDescricao + detalhes;
    }

    #endregion

    #region Persistência e montagem do DTO de resposta

    private async Task<PerfilRiscoResponse> SalvarERetornarAsync(
        int clienteId,
        PerfilRiscoTipoEnum perfil,
        int pontuacao,
        string descricao)
    {
        var dataAtual = DateTime.Now;

        // Busca perfil já existente
        var existente = await _db.PerfisClientes
            .FirstOrDefaultAsync(p => p.ClienteId == clienteId);

        if (existente == null)
        {
            existente = new PerfilCliente
            {
                ClienteId = clienteId,
                Perfil = perfil.ToString(), // grava como texto
                Pontuacao = pontuacao,
                UltimaAtualizacao = dataAtual
            };
            _db.PerfisClientes.Add(existente);
        }
        else
        {
            existente.Perfil = perfil.ToString();
            existente.Pontuacao = pontuacao;
            existente.UltimaAtualizacao = dataAtual;
        }

        await _db.SaveChangesAsync();

        // Monta DTO exatamente no padrão do enunciado (perfil, pontuacao, descricao)
        return new PerfilRiscoResponse
        {
            ClienteId = clienteId,
            Perfil = perfil.ToString(),
            PerfilTipo = perfil,
            Pontuacao = pontuacao,
            UltimaAtualizacao = dataAtual,
            Descricao = descricao
        };
    }

    #endregion
}
