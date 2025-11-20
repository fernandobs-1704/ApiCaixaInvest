using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.SwaggerExamples.Auth;

[NotMapped]
public class LoginResponseExample : IExamplesProvider<LoginResponse>
{
    public LoginResponse GetExamples()
    {
        return new LoginResponse
        {
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.exemplo.de.payload.assinatura",
            Tipo = "Bearer",
            ExpiraEm = DateTime.UtcNow.AddMinutes(30),
            Email = "caixaverso@caixa.gov.br",
            Perfil = "Usuario"
        };
    }
}
