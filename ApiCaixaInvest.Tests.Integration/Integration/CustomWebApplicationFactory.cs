using ApiCaixaInvest.Api;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiCaixaInvest.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Ambiente de teste
            builder.UseEnvironment("Testing");

            // Configuração mínima (JWT) para os testes
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Issuer"] = "ApiCaixaInvest",
                    ["Jwt:Audience"] = "ApiCaixaInvestClients",
                    ["Jwt:SecretKey"] = "bXlTdXBlclNlY3JldEtleV9Jc0hlcmVfMTIzNDU2Nzg5IQ==",
                    ["Jwt:ExpirationMinutes"] = "60"
                    // 👈 deliberadamente NÃO colocamos ConnectionStrings:Redis aqui
                });
            });

            builder.ConfigureServices(services =>
            {
                // 1) Remover DbContext real (SQLite)
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApiCaixaInvestDbContext>));

                if (dbDescriptor != null)
                    services.Remove(dbDescriptor);

                // 2) Adicionar DbContext InMemory
                services.AddDbContext<ApiCaixaInvestDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // 3) Remover TokenStore baseado em Redis
                var tokenStoreDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ITokenStore));

                if (tokenStoreDescriptor != null)
                    services.Remove(tokenStoreDescriptor);

                // 4) Remover o IConnectionMultiplexer (Redis) se existir
                var redisDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IConnectionMultiplexer));

                if (redisDescriptor != null)
                    services.Remove(redisDescriptor);

                // 5) Registrar um FakeTokenStore que NÃO usa Redis
                services.AddScoped<ITokenStore, FakeTokenStore>();
            });

            // SEED REAL – Executa APÓS criar o app real
            builder.ConfigureServices(services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApiCaixaInvestDbContext>();

                db.Database.EnsureCreated();

                if (!db.ProdutosInvestimento.Any())
                {
                    db.ProdutosInvestimento.AddRange(
                        new ProdutoInvestimento { Id = 101, Nome = "CDB Caixa", Tipo = "CDB", RentabilidadeAnual = 0.105m, Risco = "Baixo", PrazoMinimoMeses = 6 },
                        new ProdutoInvestimento { Id = 201, Nome = "Tesouro IPCA", Tipo = "Tesouro Direto", RentabilidadeAnual = 0.13m, Risco = "Médio", PrazoMinimoMeses = 36 },
                        new ProdutoInvestimento { Id = 301, Nome = "Fundo Ações", Tipo = "Fundo de Ações", RentabilidadeAnual = 0.22m, Risco = "Alto", PrazoMinimoMeses = 12 }
                    );

                    db.SaveChanges();
                }
            });
        }

        /// <summary>
        /// Implementação fake de ITokenStore para testes de integração,
        /// eliminando a dependência do Redis.
        /// </summary>
        private class FakeTokenStore : ITokenStore
        {
            public Task StoreRefreshTokenAsync(string subject, string refreshToken, DateTime expiresAt)
            {
                // Não precisamos guardar nada de verdade nos testes de integração
                return Task.CompletedTask;
            }

            public Task<bool> IsRefreshTokenValidAsync(string subject, string refreshToken)
            {
                // Para fins de teste, podemos sempre considerar válido
                return Task.FromResult(true);
            }

            public Task RevokeRefreshTokenAsync(string subject, string refreshToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
