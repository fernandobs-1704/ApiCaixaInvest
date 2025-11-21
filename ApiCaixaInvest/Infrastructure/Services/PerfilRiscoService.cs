using System.Text.Json;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Enums;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ApiCaixaInvest.Infrastructure.Services;

/// <summary>
/// Serviço responsável por calcular e persistir o perfil de risco do cliente,
/// considerando volume, frequência, liquidez e rentabilidade, conforme enunciado,
/// e enriquecendo o resultado com um modelo Markoviano de tendência futura de perfil.
/// Também grava o resultado em cache Redis (opcional), sem alterar a lógica de cálculo.
/// </summary>
public class PerfilRiscoService : IRiskProfileService
{
    private readonly ApiCaixaInvestDbContext _db;
    private readonly IDatabase? _cache;

    private const string CacheKeyPrefixPerfil = "perfil-risco:";
    private const string CacheKeyPrefixIa = "perfil-risco-ia:";

    public PerfilRiscoService(ApiCaixaInvestDbContext db, IConnectionMultiplexer? redis = null)
    {
        _db = db;
        _cache = redis?.GetDatabase();
    }

    #region Matriz de transição (Modelo Markoviano)

    /// <summary>
    /// Matriz de transição Markoviana entre perfis de risco.
    /// Cada linha representa o perfil atual e as colunas as probabilidades
    /// de migração para cada perfil (Conservador, Moderado, Agressivo).
    /// </summary>
    private static readonly Dictionary<PerfilRiscoTipoEnum, Dictionary<PerfilRiscoTipoEnum, double>> _matrizTransicao
        = new()
        {
            [PerfilRiscoTipoEnum.Conservador] = new()
            {
                [PerfilRiscoTipoEnum.Conservador] = 0.80,
                [PerfilRiscoTipoEnum.Moderado] = 0.18,
                [PerfilRiscoTipoEnum.Agressivo] = 0.02
            },
            [PerfilRiscoTipoEnum.Moderado] = new()
            {
                [PerfilRiscoTipoEnum.Conservador] = 0.10,
                [PerfilRiscoTipoEnum.Moderado] = 0.70,
                [PerfilRiscoTipoEnum.Agressivo] = 0.20
            },
            [PerfilRiscoTipoEnum.Agressivo] = new()
            {
                [PerfilRiscoTipoEnum.Conservador] = 0.03,
                [PerfilRiscoTipoEnum.Moderado] = 0.22,
                [PerfilRiscoTipoEnum.Agressivo] = 0.75
            }
        };

    private static Dictionary<string, double> ObterTendenciaPerfis(PerfilRiscoTipoEnum perfilAtual)
    {
        if (!_matrizTransicao.TryGetValue(perfilAtual, out var linha))
            return new Dictionary<string, double>();

        return linha.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
    }

    private static string? ObterProximoPerfilProvavel(PerfilRiscoTipoEnum perfilAtual)
    {
        if (!_matrizTransicao.TryGetValue(perfilAtual, out var linha) || linha.Count == 0)
            return null;

        var proximo = linha
            .OrderByDescending(kv => kv.Value)
            .First()
            .Key;

        return proximo.ToString();
    }

    #endregion

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

        var joinHistProd = (from h in historico
                            join p in produtos
                                on h.Tipo.ToLower() equals p.Tipo.ToLower()
                                into gj
                            from p in gj.DefaultIfEmpty()
                            select new { Historico = h, Produto = p })
                            .ToList();

        decimal mediaLiquidezDias = joinHistProd
            .Where(x => x.Produto != null)
            .Select(x => (decimal)x.Produto!.LiquidezDias)
            .DefaultIfEmpty(30m)
            .Average();

        decimal mediaRentabilidade = joinHistProd
            .Where(x => x.Produto != null)
            .Select(x => x.Produto!.RentabilidadeAnual)
            .DefaultIfEmpty(0.10m)
            .Average();

        // 6) Scores
        int scoreVolume = CalcularScoreVolume(totalInvestido);
        int scoreFrequencia = CalcularScoreFrequencia(frequencia12Meses);
        int scoreLiquidez = CalcularScoreLiquidez(mediaLiquidezDias);
        int scoreRentabilidade = CalcularScoreRentabilidade(mediaRentabilidade);
        int scoreRiscoCarteira = CalcularScoreRiscoCarteira(percAlta);

        // 7) Pontuação final
        int pontuacaoFinal =
            scoreVolume +
            scoreFrequencia +
            scoreLiquidez +
            scoreRentabilidade +
            scoreRiscoCarteira;

        // 8) Classificação
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

    private static int CalcularScoreVolume(decimal totalInvestido) =>
        totalInvestido switch
        {
            < 5_000m => 10,
            < 20_000m => 20,
            < 100_000m => 30,
            _ => 40
        };

    private static int CalcularScoreFrequencia(int freq12Meses) =>
        freq12Meses switch
        {
            0 => 10,
            1 => 20,
            <= 6 => 30,
            _ => 40
        };

    private static int CalcularScoreLiquidez(decimal mediaLiquidezDias) =>
        mediaLiquidezDias switch
        {
            <= 30m => 40,
            <= 90m => 25,
            _ => 10
        };

    private static int CalcularScoreRentabilidade(decimal mediaRentabilidade) =>
        mediaRentabilidade switch
        {
            < 0.08m => 10,
            < 0.12m => 20,
            < 0.20m => 30,
            _ => 40
        };

    private static int CalcularScoreRiscoCarteira(decimal percAlta) =>
        (int)Math.Round(percAlta * 40m, 0);

    #endregion

    #region Helpers de classificação de produtos

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
            || tipo.Contains("fundo");
    }

    #endregion

    #region Descrição

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

    #region Persistência + cache

    private async Task<PerfilRiscoResponse> SalvarERetornarAsync(
        int clienteId,
        PerfilRiscoTipoEnum perfil,
        int pontuacao,
        string descricao)
    {
        var dataAtual = DateTime.Now;

        var existente = await _db.PerfisClientes
            .FirstOrDefaultAsync(p => p.ClienteId == clienteId);

        if (existente == null)
        {
            existente = new PerfilCliente
            {
                ClienteId = clienteId,
                Perfil = perfil.ToString(),
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

        var tendencia = ObterTendenciaPerfis(perfil);
        var proximoPerfil = ObterProximoPerfilProvavel(perfil);

        var response = new PerfilRiscoResponse
        {
            ClienteId = clienteId,
            Perfil = perfil.ToString(),
            PerfilTipo = perfil,
            Pontuacao = pontuacao,
            UltimaAtualizacao = dataAtual,
            Descricao = descricao,
            TendenciaPerfis = tendencia,
            ProximoPerfilProvavel = proximoPerfil
        };

        // Grava no Redis (cache de leitura rápida para outros consumers)
        if (_cache != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(response);
                await _cache.StringSetAsync(
                    $"{CacheKeyPrefixPerfil}{clienteId}",
                    json,
                    TimeSpan.FromMinutes(5));
            }
            catch
            {
                // Cache não pode derrubar o serviço nem os testes.
            }
        }

        return response;
    }

    #endregion

    /// <summary>
    /// Gera explicação didática em linguagem natural
    /// e grava também no Redis (opcional).
    /// </summary>
    public async Task<PerfilRiscoIaResponse> GerarExplicacaoIaAsync(int clienteId)
    {
        if (clienteId <= 0)
            throw new ArgumentOutOfRangeException(nameof(clienteId),
                "O identificador do cliente deve ser maior que zero.");

        var perfilBase = await CalcularPerfilAsync(clienteId);

        var resumo = MontarResumo(perfilBase);
        var visaoComportamento = MontarVisaoComportamentoInvestidor(perfilBase);
        var sugestoes = MontarSugestoesEstrategicas(perfilBase);
        var acoes = MontarAcoesRecomendadas(perfilBase);
        var alertas = MontarAlertasImportantes(perfilBase);

        var ia = new PerfilRiscoIaResponse
        {
            ClienteId = perfilBase.ClienteId,
            Perfil = perfilBase.Perfil,
            Pontuacao = perfilBase.Pontuacao,
            Resumo = resumo,
            VisaoComportamentoInvestidor = visaoComportamento,
            SugestoesEstrategicas = sugestoes,
            AcoesRecomendadas = acoes,
            AlertasImportantes = alertas,
            TendenciaPerfis = perfilBase.TendenciaPerfis,
            ProximoPerfilProvavel = perfilBase.ProximoPerfilProvavel
        };

        if (_cache != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(ia);
                await _cache.StringSetAsync(
                    $"{CacheKeyPrefixIa}{clienteId}",
                    json,
                    TimeSpan.FromMinutes(5));
            }
            catch
            {
                // Mesmo esquema: falha de cache não quebra nada.
            }
        }

        return ia;
    }

    #region Helpers IA

    private static string MontarResumo(PerfilRiscoResponse perfil) =>
        perfil.PerfilTipo switch
        {
            PerfilRiscoTipoEnum.Conservador =>
                "O seu perfil é conservador, priorizando segurança, estabilidade e menor oscilação, mesmo que isso reduza o potencial de ganho.",
            PerfilRiscoTipoEnum.Moderado =>
                "O seu perfil é moderado, buscando equilíbrio entre proteção do capital e oportunidade de ganhos superiores ao longo do tempo.",
            PerfilRiscoTipoEnum.Agressivo =>
                "O seu perfil é agressivo, aceitando maior volatilidade e risco no curto prazo em troca de potencial de retorno mais elevado no futuro.",
            _ =>
                "Seu perfil foi definido a partir do comportamento recente dos seus investimentos, combinando risco e retorno."
        };

    private static string MontarVisaoComportamentoInvestidor(PerfilRiscoResponse perfil) =>
        "Seu perfil foi calculado a partir do seu histórico de investimentos, levando em conta o volume aplicado, a frequência das suas movimentações, a liquidez dos produtos que você utiliza e a sua exposição a ativos de maior risco. Essa análise reflete o seu comportamento real como investidor, e não apenas respostas teóricas de um questionário.";

    private static string MontarSugestoesEstrategicas(PerfilRiscoResponse perfil) =>
        perfil.PerfilTipo switch
        {
            PerfilRiscoTipoEnum.Conservador =>
                "Como investidor de perfil conservador, você pode fortalecer sua segurança mantendo uma reserva de emergência bem estruturada e, ao mesmo tempo, considerar incluir gradualmente uma pequena parcela em ativos moderados para reduzir o impacto da inflação.",
            PerfilRiscoTipoEnum.Moderado =>
                "Como investidor de perfil moderado, você pode obter bons resultados equilibrando sua carteira entre renda fixa e ativos de maior risco, enquanto revisa periodicamente sua alocação para evitar concentrações excessivas.",
            PerfilRiscoTipoEnum.Agressivo =>
                "Com seu perfil agressivo, você pode aproveitar oportunidades de maior retorno em estratégias de longo prazo e, ao mesmo tempo, proteger objetivos essenciais mantendo uma parte do patrimônio em produtos conservadores.",
            _ =>
                "Com seu perfil atual, uma estratégia eficiente é manter diversificação entre diferentes classes de ativos e ajustar o nível de risco conforme sua necessidade e objetivos evoluem."
        };

    private static string MontarAcoesRecomendadas(PerfilRiscoResponse perfil)
    {
        var tipo = perfil.PerfilTipo.ToString();
        var score = perfil.Pontuacao;

        return perfil.PerfilTipo switch
        {
            PerfilRiscoTipoEnum.Conservador =>
                $"Com a sua pontuação de {score} e o enquadramento no perfil {tipo}, uma boa ação é definir limites mínimos em ativos conservadores e testar, com segurança, uma pequena exposição a produtos mais arrojados para fortalecer o potencial da sua carteira no longo prazo.",
            PerfilRiscoTipoEnum.Moderado =>
                $"A partir da sua pontuação de {score} e do perfil {tipo}, é recomendado estabelecer faixas-alvo de alocação entre renda fixa e renda variável, revisando sua carteira semestralmente para garantir que ela continue alinhada aos seus objetivos.",
            PerfilRiscoTipoEnum.Agressivo =>
                $"Com a sua pontuação de {score} e o perfil {tipo}, é importante definir limites máximos de exposição a ativos muito voláteis e adotar rebalanceamentos periódicos para evitar que eventos de mercado deixem sua carteira mais arriscada do que você gostaria.",
            _ =>
                $"Considerando sua pontuação de {score} e o perfil {tipo}, uma ação útil é mapear metas de alocação por prazo e acompanhar de tempos em tempos se a distribuição dos seus investimentos continua compatível com sua classificação."
        };
    }

    private static string MontarAlertasImportantes(PerfilRiscoResponse perfil)
    {
        string alertaBase = perfil.PerfilTipo switch
        {
            PerfilRiscoTipoEnum.Conservador =>
                "Como investidor conservador, é importante ficar atento ao risco de sua rentabilidade ficar abaixo da inflação quando há concentração excessiva em produtos de curtíssimo prazo.",
            PerfilRiscoTipoEnum.Moderado =>
                "Com seu perfil moderado, é essencial acompanhar a parcela mais exposta ao risco, já que oscilações maiores podem ocorrer se a carteira não for revisada periodicamente.",
            PerfilRiscoTipoEnum.Agressivo =>
                "Com seu perfil agressivo, o principal alerta está na possibilidade de perdas relevantes durante períodos de forte volatilidade, especialmente se você não mantiver uma reserva em ativos conservadores.",
            _ =>
                "Com seu perfil atual, é importante garantir que sua exposição ao risco esteja alinhada aos seus objetivos financeiros e tolerância a variações."
        };

        if (!string.IsNullOrWhiteSpace(perfil.ProximoPerfilProvavel))
        {
            alertaBase +=
                $" Há também tendência de migração para o perfil {perfil.ProximoPerfilProvavel}, o que indica que seu comportamento atual pode estar alterando seu nível natural de tolerância ao risco.";
        }

        alertaBase +=
            " Mudanças na sua vida financeira ou nos seus objetivos podem exigir uma reavaliação completa do seu perfil.";

        return alertaBase;
    }

    #endregion
}
