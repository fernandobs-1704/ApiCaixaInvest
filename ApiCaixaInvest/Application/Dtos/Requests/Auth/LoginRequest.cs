namespace ApiCaixaInvest.Application.Dtos.Requests.Auth;

public class LoginRequest
{
    /// <summary>
    /// E-mail do usuário para autenticação.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário para autenticação.
    /// </summary>
    public string Senha { get; set; } = string.Empty;
}
