using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;

namespace ApiCaixaInvest.Application.Interfaces;

public interface IRiskProfileService
{
    Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId);
    Task<PerfilRiscoIaResponse> GerarExplicacaoIaAsync(int clienteId);
}
