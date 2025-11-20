using System.Linq;
using System.Threading.Tasks;
using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class ClientesControllerTests : TestBase
    {
        [Fact]
        public async Task GetClientes_DeveRetornarListaOrdenada()
        {
            using var ctx = CreateContext();

            ctx.Clientes.Add(new Cliente { Id = 2 });
            ctx.Clientes.Add(new Cliente { Id = 1 });
            await ctx.SaveChangesAsync();

            var controller = new ClientesController(ctx);

            // Act
            var result = await controller.GetClientes();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var lista = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Cliente>>(ok.Value);
            var ids = lista.Select(c => c.Id).ToList();

            Assert.Equal(2, ids.Count);
            Assert.Equal(1, ids[0]);
            Assert.Equal(2, ids[1]);
        }

        [Fact]
        public async Task GetClientePorId_DeveRetornarBadRequest_QuandoIdInvalido()
        {
            using var ctx = CreateContext();
            var controller = new ClientesController(ctx);

            var result = await controller.GetClientePorId(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(bad.Value);
        }

        [Fact]
        public async Task GetClientePorId_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            using var ctx = CreateContext();
            var controller = new ClientesController(ctx);

            var result = await controller.GetClientePorId(123);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFound.Value);
        }

        [Fact]
        public async Task GetClientePorId_DeveRetornarOk_QuandoEncontrado()
        {
            using var ctx = CreateContext();
            ctx.Clientes.Add(new Cliente { Id = 10 });
            await ctx.SaveChangesAsync();

            var controller = new ClientesController(ctx);

            var result = await controller.GetClientePorId(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var cliente = Assert.IsType<Cliente>(ok.Value);
            Assert.Equal(10, cliente.Id);
        }
    }
}
