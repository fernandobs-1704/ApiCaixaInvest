using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Api.SwaggerExamples.PerfilRisco;

[NotMapped]
public class PerfilRiscoResponseExample : IExamplesProvider<PerfilRiscoResponse>
{
    public PerfilRiscoResponse GetExamples()
    {
        return new PerfilRiscoResponse
        {
            ClienteId = 123,
            Perfil = "Moderado",
            PerfilTipo = PerfilRiscoTipoEnum.Moderado,
            Pontuacao = 65,
            Descricao = "Perfil equilibrado entre segurança e rentabilidade.",
            UltimaAtualizacao = DateTime.UtcNow
        };
    }
}
