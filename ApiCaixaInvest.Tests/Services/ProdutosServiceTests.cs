using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCaixaInvest.Tests.Services
{
    public class ProdutosServiceTests : TestBase
    {
        [Fact]
        public async Task ObterProdutosRecomendadosAsync_DeveLancarArgumentException_QuandoPerfilVazio()
        {
            using var ctx = CreateContext();
            var service = new ProdutosService(ctx);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ObterProdutosRecomendadosAsync(""));
        }

        [Fact]
        public async Task ObterProdutosRecomendadosAsync_DeveLancarInvalidOperation_QuandoPerfilInvalido()
        {
            using var ctx = CreateContext();
            var service = new ProdutosService(ctx);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ObterProdutosRecomendadosAsync("XPTO"));
        }

        [Fact]
        public async Task ObterProdutosRecomendadosAsync_Conservador_DeveRetornarSomenteBaixoRisco()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento { Nome = "CDB1", Tipo = "CDB", RentabilidadeAnual = 0.1m, Risco = "Baixo" },
                new ProdutoInvestimento { Nome = "Fundo A", Tipo = "Fundo", RentabilidadeAnual = 0.2m, Risco = "Alto" },
                new ProdutoInvestimento { Nome = "LCI1", Tipo = "LCI", RentabilidadeAnual = 0.11m, Risco = "Baixo" }
            );
            await ctx.SaveChangesAsync();

            var service = new ProdutosService(ctx);

            // Act
            var produtos = await service.ObterProdutosRecomendadosAsync("Conservador");

            // Assert
            Assert.True(produtos.All(p => p.Risco == "Baixo"));
        }

        [Fact]
        public async Task ObterProdutosRecomendadosAsync_Moderado_DeveIncluirBaixoEMedio()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento { Nome = "CDB1", Tipo = "CDB", RentabilidadeAnual = 0.1m, Risco = "Baixo" },
                new ProdutoInvestimento { Nome = "CDB2", Tipo = "CDB", RentabilidadeAnual = 0.12m, Risco = "Médio" },
                new ProdutoInvestimento { Nome = "CDB3", Tipo = "CDB", RentabilidadeAnual = 0.15m, Risco = "Alto" }
            );
            await ctx.SaveChangesAsync();

            var service = new ProdutosService(ctx);

            // Act
            var produtos = await service.ObterProdutosRecomendadosAsync("moderado");

            // Assert
            Assert.True(produtos.All(p => p.Risco == "Baixo" || p.Risco == "Médio" || p.Risco == "Medio"));
            Assert.DoesNotContain(produtos, p => p.Risco == "Alto");
        }

        [Fact]
        public async Task ObterProdutosRecomendadosAsync_Agressivo_DeveRetornarTodosOsRiscos()
        {
            using var ctx = CreateContext();

            ctx.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento { Nome = "CDB1", Tipo = "CDB", RentabilidadeAnual = 0.1m, Risco = "Baixo" },
                new ProdutoInvestimento { Nome = "CDB2", Tipo = "CDB", RentabilidadeAnual = 0.12m, Risco = "Médio" },
                new ProdutoInvestimento { Nome = "Fundo A", Tipo = "Fundo", RentabilidadeAnual = 0.2m, Risco = "Alto" }
            );
            await ctx.SaveChangesAsync();

            var service = new ProdutosService(ctx);

            // Act
            var produtos = await service.ObterProdutosRecomendadosAsync("Agressivo");

            // Assert
            Assert.Equal(3, produtos.Count);
        }
    }
}
