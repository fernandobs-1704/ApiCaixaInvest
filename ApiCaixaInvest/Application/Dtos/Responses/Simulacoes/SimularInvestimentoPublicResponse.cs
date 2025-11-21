using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Responses.Simulacoes
{
    /// <summary>
    /// Resposta da simulação de investimento, exatamente como no enunciado
    /// </summary>
    [NotMapped]
    public class SimularInvestimentoPublicResponse
    {
        /// <summary>
        /// Produto que foi selecionado/validado de acordo com os parâmetros informados.
        /// </summary>
        public ProdutoResponse ProdutoValidado { get; set; } = new();

        /// <summary>
        /// Resultado da simulação financeira (valor final, rentabilidade, prazo).
        /// </summary>
        public ResultadoSimulacaoResponse ResultadoSimulacao { get; set; } = new();

        /// <summary>
        /// Data e hora em que a simulação foi realizada.
        /// (Campo extra, mas não conflita com o enunciado.)
        /// </summary>
        public DateTime DataSimulacao { get; set; }
    }
}
