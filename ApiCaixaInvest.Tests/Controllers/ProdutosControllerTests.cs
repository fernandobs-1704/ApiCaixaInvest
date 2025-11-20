using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCaixaInvest.Api.Controllers;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class ProdutosControllerTests : TestBase
    {
        [Fact]
        public async Task GetProdutos_DeveRetornarListaOrdenadaPorRiscoENome()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Id = 1,
                Nome = "Produto B",
                Risco = "Baixo"
            });
            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Id = 2,
                Nome = "Produto A",
                Risco = "Baixo"
            });

            await ctx.SaveChangesAsync();

            var controller = new ProdutosController(ctx);

            var result = await controller.GetProdutos();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<ProdutoInvestimento>>(ok.Value);
            var nomes = lista.Select(p => p.Nome).ToList();

            Assert.Equal(2, nomes.Count);
            Assert.Equal("Produto A", nomes[0]);
            Assert.Equal("Produto B", nomes[1]);
        }

        [Fact]
        public async Task GetProdutoPorId_DeveRetornarNotFound_QuandoNaoExiste()
        {
            using var ctx = CreateContext();
            var controller = new ProdutosController(ctx);

            var result = await controller.GetProdutoPorId(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFound.Value);
        }

        [Fact]
        public async Task GetProdutoPorId_DeveRetornarOk_QuandoExiste()
        {
            using var ctx = CreateContext();
            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Id = 10,
                Nome = "CDB Banco X",
                Risco = "Baixo"
            });
            await ctx.SaveChangesAsync();

            var controller = new ProdutosController(ctx);

            var result = await controller.GetProdutoPorId(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var produto = Assert.IsType<ProdutoInvestimento>(ok.Value);
            Assert.Equal(10, produto.Id);
            Assert.Equal("CDB Banco X", produto.Nome);
        }

        [Fact]
        public async Task GetProdutosPorRisco_DeveFiltrarCorretamente()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Id = 1,
                Nome = "CDB Conservador",
                Risco = "Baixo"
            });
            ctx.ProdutosInvestimento.Add(new ProdutoInvestimento
            {
                Id = 2,
                Nome = "Fundo Agressivo",
                Risco = "Alto"
            });
            await ctx.SaveChangesAsync();

            var controller = new ProdutosController(ctx);

            var result = await controller.GetProdutosPorRisco("Baixo");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var lista = Assert.IsAssignableFrom<IEnumerable<ProdutoInvestimento>>(ok.Value);
            var produtos = lista.ToList();

            Assert.Single(produtos);
            Assert.Equal("CDB Conservador", produtos[0].Nome);
            Assert.Equal("Baixo", produtos[0].Risco);
        }
    }
}
