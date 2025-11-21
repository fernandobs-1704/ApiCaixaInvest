namespace ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco
{
    /// <summary>
    /// Resposta em linguagem natural (estilo IA) para o perfil de risco,
    /// baseada no cálculo já realizado e nas tendências de migração de perfil.
    /// </summary>
    public class PerfilRiscoIaResponse
    {
        public int ClienteId { get; set; }
        public string Perfil { get; set; } = default!;
        public int Pontuacao { get; set; }

        /// <summary>
        /// Resumo em uma frase, pronto para uso em cards ou highlights.
        /// </summary>
        public string Resumo { get; set; } = default!;

        /// <summary>
        /// Visão geral do comportamento do investidor a partir dos dados analisados.
        /// </summary>
        public string VisaoComportamentoInvestidor { get; set; } = default!;

        /// <summary>
        /// Sugestões estratégicas em uma única frase didática, com duas recomendações combinadas.
        /// </summary>
        public string SugestoesEstrategicas { get; set; } = default!;

        /// <summary>
        /// Ações recomendadas, personalizadas pela pontuação e perfil atual.
        /// </summary>
        public string AcoesRecomendadas { get; set; } = default!;

        /// <summary>
        /// Alertas importantes, considerando pontos críticos como possível mudança de perfil.
        /// </summary>
        public string AlertasImportantes { get; set; } = default!;

        /// <summary>
        /// Tendência de migração entre perfis, baseada na matriz de transição.
        /// </summary>
        public Dictionary<string, double>? TendenciaPerfis { get; set; }

        /// <summary>
        /// Próximo perfil mais provável, considerando o comportamento atual.
        /// </summary>
        public string? ProximoPerfilProvavel { get; set; }
    }
}
