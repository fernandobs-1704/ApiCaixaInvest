using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using Swashbuckle.AspNetCore.Filters;

namespace ApiCaixaInvest.Api.SwaggerExamples.Auth;

public class LoginResponseExample : IExamplesProvider<LoginResponse>
{
    public LoginResponse GetExamples()
    {
        return new LoginResponse
        {
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.exemplo.de.payload.assinatura",
            Tipo = "Bearer",
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            Email = "cliente@caixa.gov.br",
            Perfil = "Cliente"
        };
    }
}
