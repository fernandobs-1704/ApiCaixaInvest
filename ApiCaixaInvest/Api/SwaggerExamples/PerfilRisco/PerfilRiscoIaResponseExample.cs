using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace ApiCaixaInvest.Api.SwaggerExamples.PerfilRisco
{
    /// <summary>
    /// Exemplo de resposta didática para o endpoint /api/perfil-risco-ia/{clienteId}.
    /// </summary>
    public class PerfilRiscoIaResponseExample : IExamplesProvider<PerfilRiscoIaResponse>
    {
        public PerfilRiscoIaResponse GetExamples()
        {
            return new PerfilRiscoIaResponse
            {
                ClienteId = 123,
                Perfil = "Moderado",
                Pontuacao = 112,

                Resumo = "O seu perfil é moderado, buscando equilíbrio entre proteção do capital e oportunidade de ganhos superiores ao longo do tempo.",

                VisaoComportamentoInvestidor =
                    "Seu perfil foi calculado a partir do seu histórico de investimentos, levando em conta o volume aplicado, a frequência das suas movimentações, a liquidez dos produtos que você utiliza e a sua exposição a ativos de maior risco. Essa análise reflete o seu comportamento real como investidor, e não apenas respostas teóricas de um questionário.",

                SugestoesEstrategicas =
                    "Como investidor de perfil moderado, você pode obter bons resultados equilibrando sua carteira entre renda fixa e ativos de maior risco, enquanto revisa periodicamente sua alocação para evitar concentrações excessivas.",

                AcoesRecomendadas =
                    "A partir da sua pontuação de 112 e do perfil Moderado, é recomendado estabelecer faixas-alvo de alocação entre renda fixa e renda variável, revisando sua carteira semestralmente para garantir que ela continue alinhada aos seus objetivos.",

                AlertasImportantes =
                    "Com seu perfil moderado, é essencial acompanhar a parcela mais exposta ao risco, já que oscilações maiores podem ocorrer se a carteira não for revisada periodicamente. Há também tendência de migração para o perfil Agressivo, o que indica que seu comportamento atual pode estar elevando gradualmente seu nível de tolerância ao risco. Mudanças na sua vida financeira ou nos seus objetivos podem exigir uma reavaliação completa do seu perfil.",

                TendenciaPerfis = new Dictionary<string, double>
                {
                    { "Conservador", 0.18 },
                    { "Moderado",    0.70 },
                    { "Agressivo",   0.12 }
                },

                ProximoPerfilProvavel = "Moderado"
            };
        }
    }
}
