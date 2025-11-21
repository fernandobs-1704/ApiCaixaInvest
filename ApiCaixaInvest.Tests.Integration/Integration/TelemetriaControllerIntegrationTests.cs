using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Telemetria;
using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class TelemetriaControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public TelemetriaControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient() => _factory.CreateClient();

        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = CreateClient();
            var request = new LoginRequest
            {
                Email = "caixaverso@caixa.gov.br",
                Senha = "Caixaverso@2025"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(body);
            Assert.False(string.IsNullOrWhiteSpace(body!.Token));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", body.Token);

            return client;
        }

        [Fact]
        public async Task GetTelemetria_ComRegistros_DeveRetornarResumo()
        {
            // Primeiro: popula Telemetria direto no contexto
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApiCaixaInvestDbContext>();

                if (!db.TelemetriaRegistros.Any())
                {
                    db.TelemetriaRegistros.AddRange(
                        new TelemetriaRegistro
                        {
                            Servico = "GET /api/simular-investimento",
                            TempoRespostaMs = 120,
                            Data = new DateTime(2025, 1, 10)
                        },
                        new TelemetriaRegistro
                        {
                            Servico = "GET /api/simular-investimento",
                            TempoRespostaMs = 80,
                            Data = new DateTime(2025, 1, 10)
                        },
                        new TelemetriaRegistro
                        {
                            Servico = "POST /api/auth/login",
                            TempoRespostaMs = 50,
                            Data = new DateTime(2025, 1, 15)
                        }
                    );

                    db.SaveChanges();
                }
            }

            var client = await CreateAuthenticatedClientAsync();

            var inicio = new DateOnly(2025, 1, 1);
            var fim = new DateOnly(2025, 1, 31);

            var url = $"/api/telemetria?inicio={inicio:yyyy-MM-dd}&fim={fim:yyyy-MM-dd}";

            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resumo = await response.Content.ReadFromJsonAsync<TelemetriaResponse>();
            Assert.NotNull(resumo);
            Assert.NotNull(resumo!.Servicos);
            Assert.True(resumo.Servicos.Any());

            // Deve conter pelo menos o serviço de simulação
            Assert.Contains(resumo.Servicos, s =>
                s.Nome.Contains("simular-investimento", StringComparison.OrdinalIgnoreCase));
        }
    }
}
