namespace ApiCaixaInvest.Domain.Enums
{
    /// <summary>
    /// Representa o tipo de perfil de risco calculado para um cliente.
    /// </summary>
    public enum PerfilRiscoTipoEnum
    {
        /// <summary>
        /// Perfil Conservador: prioriza segurança e liquidez.
        /// </summary>
        Conservador = 1,

        /// <summary>
        /// Perfil Moderado: equilíbrio entre liquidez e rentabilidade.
        /// </summary>
        Moderado = 2,

        /// <summary>
        /// Perfil Agressivo: busca alta rentabilidade assumindo maior risco.
        /// </summary>
        Agressivo = 3
    }
}
