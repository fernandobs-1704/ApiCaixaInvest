using System;
using System.Linq;
using System.Threading.Tasks;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Domain.Enums;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using ApiCaixaInvest.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApiCaixaInvest.Tests.Services
{
    public class PerfilRiscoServiceTests : TestBase
    {
        [Fact]
        public async Task CalcularPerfilAsync_sem_historico_retorna_conservador()
        {
            // Arrange
            using var db = CreateContext();
            var service = new PerfilRiscoService(db);
            int clienteId = 123;

            // Act
            PerfilRiscoResponse resultado = await service.CalcularPerfilAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(clienteId, resultado.ClienteId);
            Assert.Equal(PerfilRiscoTipoEnum.Conservador, resultado.PerfilTipo);
            Assert.Equal("Conservador", resultado.Perfil);
            Assert.Equal(20, resultado.Pontuacao);
            Assert.Contains("conservador", resultado.Descricao, StringComparison.OrdinalIgnoreCase);

            var perfilDb = await db.PerfisClientes.SingleAsync(p => p.ClienteId == clienteId);
            Assert.Equal("Conservador", perfilDb.Perfil);
            Assert.Equal(20, perfilDb.Pontuacao);
        }

        [Fact]
        public async Task CalcularPerfilAsync_total_investido_zero_retorna_conservador()
        {
            // Arrange
            using var db = CreateContext();
            var service = new PerfilRiscoService(db);
            int clienteId = 200;

            db.InvestimentosHistorico.Add(new InvestimentoHistorico
            {
                ClienteId = clienteId,
                Tipo = "CDB",
                Valor = 0m,
                Rentabilidade = 0.10m,
                Data = DateTime.Now.AddMonths(-2)
            });

            await db.SaveChangesAsync();

            // Act
            var resultado = await service.CalcularPerfilAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(clienteId, resultado.ClienteId);
            Assert.Equal(PerfilRiscoTipoEnum.Conservador, resultado.PerfilTipo);
            Assert.Equal("Conservador", resultado.Perfil);
            Assert.Equal(20, resultado.Pontuacao);
            Assert.Contains("Total investido igual ou inferior a zero", resultado.Descricao, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CalcularPerfilAsync_carteira_equilibrada_retorna_moderado()
        {
            // Arrange
            using var db = CreateContext();
            var service = new PerfilRiscoService(db);
            int clienteId = 300;

            // Produtos (para liquidez e rentabilidade)
            db.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento
                {
                    Id = 101,
                    Nome = "CDB Caixa 2026",
                    Tipo = "CDB",
                    RentabilidadeAnual = 0.10m,
                    Risco = "Baixo",
                    PrazoMinimoMeses = 6,
                    LiquidezDias = 30
                },
                new ProdutoInvestimento
                {
                    Id = 201,
                    Nome = "Fundo Multimercado XPTO",
                    Tipo = "Fundo Multimercado",
                    RentabilidadeAnual = 0.18m,
                    Risco = "Alto",
                    PrazoMinimoMeses = 6,
                    LiquidezDias = 90
                }
            );

            // Histórico: metade renda fixa, metade multimercado
            db.InvestimentosHistorico.AddRange(
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "CDB",
                    Valor = 10_000m,
                    Rentabilidade = 0.10m,
                    Data = DateTime.Now.AddMonths(-6)
                },
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "Fundo Multimercado",
                    Valor = 10_000m,
                    Rentabilidade = 0.18m,
                    Data = DateTime.Now.AddMonths(-3)
                },
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "CDB",
                    Valor = 0m,
                    Rentabilidade = 0.10m,
                    Data = DateTime.Now.AddMonths(-1)
                }
            );

            await db.SaveChangesAsync();

            // Act
            var resultado = await service.CalcularPerfilAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(clienteId, resultado.ClienteId);
            Assert.Equal(PerfilRiscoTipoEnum.Moderado, resultado.PerfilTipo);
            Assert.Equal("Moderado", resultado.Perfil);

            // Deve cair na faixa de moderado (81 a 140 pontos)
            Assert.InRange(resultado.Pontuacao, 81, 140);
            Assert.Contains("moderado", resultado.Descricao, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CalcularPerfilAsync_carteira_alto_risco_retorna_agressivo()
        {
            // Arrange
            using var db = CreateContext();
            var service = new PerfilRiscoService(db);
            int clienteId = 400;

            // Produtos
            db.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento
                {
                    Id = 101,
                    Nome = "CDB Liquidez Diária",
                    Tipo = "CDB",
                    RentabilidadeAnual = 0.10m,
                    Risco = "Baixo",
                    PrazoMinimoMeses = 3,
                    LiquidezDias = 30
                },
                new ProdutoInvestimento
                {
                    Id = 202,
                    Nome = "Fundo Multimercado Agressivo",
                    Tipo = "Fundo Multimercado",
                    RentabilidadeAnual = 0.22m,
                    Risco = "Alto",
                    PrazoMinimoMeses = 12,
                    LiquidezDias = 180
                }
            );

            // Histórico: maior parte em fundo agressivo, volume alto, frequência alta
            db.InvestimentosHistorico.AddRange(
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "Fundo Multimercado",
                    Valor = 80_000m,
                    Rentabilidade = 0.22m,
                    Data = DateTime.Now.AddMonths(-2)
                },
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "Fundo Multimercado",
                    Valor = 10_000m,
                    Rentabilidade = 0.22m,
                    Data = DateTime.Now.AddMonths(-1)
                },
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "CDB",
                    Valor = 10_000m,
                    Rentabilidade = 0.10m,
                    Data = DateTime.Now.AddMonths(-5)
                }
            );

            await db.SaveChangesAsync();

            // Act
            var resultado = await service.CalcularPerfilAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(clienteId, resultado.ClienteId);
            Assert.Equal(PerfilRiscoTipoEnum.Agressivo, resultado.PerfilTipo);
            Assert.Equal("Agressivo", resultado.Perfil);

            // Agressivo => pontuação acima de 140
            Assert.True(resultado.Pontuacao > 140);
            Assert.Contains("agressivo", resultado.Descricao, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CalcularPerfilAsync_atualiza_registro_existente_de_perfil_cliente()
        {
            // Arrange
            using var db = CreateContext();
            var service = new PerfilRiscoService(db);
            int clienteId = 500;

            // Produtos
            db.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento
                {
                    Id = 101,
                    Nome = "CDB Caixa 2026",
                    Tipo = "CDB",
                    RentabilidadeAnual = 0.10m,
                    Risco = "Baixo",
                    PrazoMinimoMeses = 6,
                    LiquidezDias = 30
                },
                new ProdutoInvestimento
                {
                    Id = 202,
                    Nome = "Fundo Multimercado Agressivo",
                    Tipo = "Fundo Multimercado",
                    RentabilidadeAnual = 0.22m,
                    Risco = "Alto",
                    PrazoMinimoMeses = 12,
                    LiquidezDias = 180
                }
            );

            // Perfil pré-existente (por exemplo Conservador)
            db.PerfisClientes.Add(new PerfilCliente
            {
                ClienteId = clienteId,
                Perfil = "Conservador",
                Pontuacao = 20,
                UltimaAtualizacao = DateTime.Now.AddDays(-10)
            });

            // Histórico inicial mais conservador
            db.InvestimentosHistorico.Add(new InvestimentoHistorico
            {
                ClienteId = clienteId,
                Tipo = "CDB",
                Valor = 10_000m,
                Rentabilidade = 0.10m,
                Data = DateTime.Now.AddMonths(-3)
            });

            await db.SaveChangesAsync();

            // Act 1: primeiro cálculo (provavelmente moderado ou conservador)
            var resultado1 = await service.CalcularPerfilAsync(clienteId);

            // Agora adiciona investimentos agressivos
            db.InvestimentosHistorico.Add(new InvestimentoHistorico
            {
                ClienteId = clienteId,
                Tipo = "Fundo Multimercado",
                Valor = 90_000m,
                Rentabilidade = 0.22m,
                Data = DateTime.Now.AddMonths(-1)
            });

            await db.SaveChangesAsync();

            // Act 2: recálculo com carteira muito mais agressiva
            var resultado2 = await service.CalcularPerfilAsync(clienteId);

            // Assert
            var perfis = await db.PerfisClientes.Where(p => p.ClienteId == clienteId).ToListAsync();
            Assert.Single(perfis); // continua um único registro no banco

            var perfilDb = perfis.Single();
            Assert.Equal(resultado2.Perfil, perfilDb.Perfil);
            Assert.Equal(resultado2.Pontuacao, perfilDb.Pontuacao);

            // Em geral, a pontuação deve ter aumentado após os investimentos mais arriscados
            Assert.True(resultado2.Pontuacao >= resultado1.Pontuacao);
        }

        [Fact]
        public async Task CalcularPerfilAsync_funciona_com_tipos_case_insensitive()
        {
            // Arrange
            using var db = CreateContext();
            var service = new PerfilRiscoService(db);
            int clienteId = 600;

            // Produtos com tipos em caixa normal
            db.ProdutosInvestimento.AddRange(
                new ProdutoInvestimento
                {
                    Id = 1,
                    Nome = "CDB Caixa 2026",
                    Tipo = "CDB",
                    RentabilidadeAnual = 0.10m,
                    Risco = "Baixo",
                    PrazoMinimoMeses = 6,
                    LiquidezDias = 30
                },
                new ProdutoInvestimento
                {
                    Id = 2,
                    Nome = "Fundo Multimercado XPTO",
                    Tipo = "Fundo Multimercado",
                    RentabilidadeAnual = 0.18m,
                    Risco = "Alto",
                    PrazoMinimoMeses = 6,
                    LiquidezDias = 60
                }
            );

            // Histórico usando variações de caixa no Tipo
            db.InvestimentosHistorico.AddRange(
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "cDb",
                    Valor = 5_000m,
                    Rentabilidade = 0.10m,
                    Data = DateTime.Now.AddMonths(-4)
                },
                new InvestimentoHistorico
                {
                    ClienteId = clienteId,
                    Tipo = "FUNDO MULTIMERCADO",
                    Valor = 5_000m,
                    Rentabilidade = 0.18m,
                    Data = DateTime.Now.AddMonths(-2)
                }
            );

            await db.SaveChangesAsync();

            // Act
            var resultado = await service.CalcularPerfilAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(clienteId, resultado.ClienteId);

            // O importante aqui é garantir que não quebre e que monte descrição coerente
            Assert.True(resultado.Pontuacao > 0);
            Assert.False(string.IsNullOrWhiteSpace(resultado.Descricao));
        }
    }
}
