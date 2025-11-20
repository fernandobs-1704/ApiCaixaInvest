using ApiCaixaInvest.Domain.Models;

namespace ApiCaixaInvest.Application.Interfaces
{
    public interface IClienteService
    {
        Task<Cliente> GarantirClienteAsync(int clienteId);
    }
}
