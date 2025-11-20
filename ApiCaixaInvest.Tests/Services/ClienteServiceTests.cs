using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCaixaInvest.Tests.Services
{
    public class ClienteServiceTests : TestBase
    {
        [Fact]
        public async Task GarantirClienteAsync_DeveCriarCliente_QuandoNaoExiste()
        {
            using var ctx = CreateContext();
            var service = new ClienteService(ctx);

            // Act
            var cliente = await service.GarantirClienteAsync(10);

            // Assert
            Assert.NotNull(cliente);
            Assert.Equal(10, cliente.Id);

            var noDb = await ctx.Clientes.FindAsync(10);
            Assert.NotNull(noDb);
        }

        [Fact]
        public async Task GarantirClienteAsync_DeveRetornarClienteExistente()
        {
            using var ctx = CreateContext();

            ctx.Clientes.Add(new Cliente
            {
                Id = 5,
                DataCriacao = DateTime.Now.AddDays(-1)
            });
            await ctx.SaveChangesAsync();

            var service = new ClienteService(ctx);

            // Act
            var cliente = await service.GarantirClienteAsync(5);

            // Assert
            Assert.NotNull(cliente);
            Assert.Equal(5, cliente.Id);

            var total = await ctx.Clientes.CountAsync(c => c.Id == 5);
            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GarantirClienteAsync_DeveLancarExcecao_QuandoIdInvalido()
        {
            using var ctx = CreateContext();
            var service = new ClienteService(ctx);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GarantirClienteAsync(0));
        }
    }
}
