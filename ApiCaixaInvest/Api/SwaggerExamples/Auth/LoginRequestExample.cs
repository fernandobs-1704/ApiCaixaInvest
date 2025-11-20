using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.SwaggerExamples.Auth;

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
