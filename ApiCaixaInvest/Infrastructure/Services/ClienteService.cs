using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApiCaixaInvestDbContext _db;

    public ClienteService(ApiCaixaInvestDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Garante que o cliente exista na base. Se não existir, cria.
    /// </summary>
    public async Task<Cliente> GarantirClienteAsync(int clienteId)
    {
        if (clienteId <= 0)
            throw new ArgumentException("O identificador do cliente deve ser maior que zero.");

        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId);

        if (cliente != null)
            return cliente;

        // Criar cliente minimalista
        cliente = new Cliente
        {
            Id = clienteId,
            DataCriacao = DateTime.Now
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        return cliente;
    }
}
