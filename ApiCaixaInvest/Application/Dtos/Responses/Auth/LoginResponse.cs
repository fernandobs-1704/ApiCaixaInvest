namespace ApiCaixaInvest.Application.Dtos.Responses.Auth;

public class LoginResponse
{
    /// <summary>
    /// Token JWT gerado após autenticação.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do token (sempre "Bearer").
    /// </summary>
    public string Tipo { get; set; } = "Bearer";

    /// <summary>
    /// Data/hora de expiração do token (UTC).
    /// </summary>
    public DateTime ExpiraEm { get; set; }

    /// <summary>
    /// E-mail autenticado.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Perfil/regra de acesso (ex: Cliente, Gestor).
    /// </summary>
    public string Perfil { get; set; } = string.Empty;
}
