using ApiCaixaInvest.Api;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace ApiCaixaInvest.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Issuer"] = "ApiCaixaInvest",
                    ["Jwt:Audience"] = "ApiCaixaInvestClients",
                    ["Jwt:SecretKey"] = "bXlTdXBlclNlY3JldEtleV9Jc0hlcmVfMTIzNDU2Nzg5IQ==",
                    ["Jwt:ExpirationMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove DbContext real
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApiCaixaInvestDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // Adiciona InMemory
                services.AddDbContext<ApiCaixaInvestDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
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
    }
}
