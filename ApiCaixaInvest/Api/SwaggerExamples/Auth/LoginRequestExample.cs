using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.SwaggerExamples.Auth;

[NotMapped]
public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Email = "caixaverso@caixa.gov.br",
            Senha = "Caixaverso@2025"
        };
    }
}
