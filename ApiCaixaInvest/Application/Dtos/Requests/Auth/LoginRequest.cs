using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Application.Dtos.Requests.Auth;

/// <summary>
/// Dados de entrada para autenticação via JWT.
/// </summary>
[NotMapped]
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
