using ApiCaixaInvest.Api;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace ApiCaixaInvest.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configura o host para testes
        builder.ConfigureServices(services =>
        {
            // Remove todos os DbContexts
            var descriptors = services
                .Where(d => d.ServiceType.BaseType == typeof(DbContext) ||
                           d.ServiceType == typeof(DbContextOptions) ||
                           d.ServiceType.Name.Contains("DbContext"))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Adiciona InMemory
            services.AddDbContext<ApiCaixaInvestDbContext>(options =>
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                ["Jwt:Issuer"] = "ApiCaixaInvest-Test",
                ["Jwt:Audience"] = "ApiCaixaInvestClients-Test",
                ["Jwt:SecretKey"] = "bXlTdXBlclNlY3JldEtleV9Jc0hlcmVfMTIzNDU2Nzg5IQ==",
                ["Jwt:ExpirationMinutes"] = "60"
            });
        });
    }
}