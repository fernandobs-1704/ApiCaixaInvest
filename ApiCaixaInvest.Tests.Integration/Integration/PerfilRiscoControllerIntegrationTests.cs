using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class PerfilRiscoControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public PerfilRiscoControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task ObterPerfilRisco_AposSimularEContratar_DeveRetornar200()
        {
            var client = await CreateAuthenticatedClientAsync();
            var clienteId = 3456;

            var request = new SimularInvestimentoRequest
            {
                ClienteId = clienteId,
                Valor = 1500m,
                PrazoMeses = 18,
                TipoProduto = "CDB"
            };

            // Usa o endpoint que já simula e contrata
            var simContratarResp = await client.PostAsJsonAsync("/api/simular-e-contratar-investimento", request);
            Assert.Equal(HttpStatusCode.OK, simContratarResp.StatusCode);

            // Agora consulta o perfil de risco
            var perfilResp = await client.GetAsync($"/api/perfil-risco/{clienteId}");
            Assert.Equal(HttpStatusCode.OK, perfilResp.StatusCode);

            var perfil = await perfilResp.Content.ReadFromJsonAsync<PerfilRiscoResponse>();
            Assert.NotNull(perfil);
            Assert.False(string.IsNullOrWhiteSpace(perfil!.Perfil));
        }
    }
}
