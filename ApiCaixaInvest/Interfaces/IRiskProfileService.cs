using ApiCaixaInvest.Dtos.Responses.PerfilRisco;

namespace ApiCaixaInvest.Interfaces;

public interface IRiskProfileService
{
    Task<PerfilRiscoResponse> CalcularPerfilAsync(int clienteId);
}
