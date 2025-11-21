using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using ApiCaixaInvest.Domain.Models;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class ClientesControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ClientesControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task GetClientes_AposSimulacao_DeveConterCliente()
        {
            var client = await CreateAuthenticatedClientAsync();

            var clienteId = 9876;

            // Gera uma simulação para forçar criação do cliente na base
            var simRequest = new SimularInvestimentoRequest
            {
                ClienteId = clienteId,
                Valor = 1000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var simResponse = await client.PostAsJsonAsync("/api/simular-investimento", simRequest);
            Assert.Equal(HttpStatusCode.OK, simResponse.StatusCode);

            // Consulta lista de clientes
            var response = await client.GetAsync("/api/clientes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var clientes = await response.Content.ReadFromJsonAsync<IEnumerable<Cliente>>();
            Assert.NotNull(clientes);
            Assert.Contains(clientes!, c => c.Id == clienteId);
        }

        [Fact]
        public async Task GetClientePorId_Inexistente_DeveRetornar404()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/clientes/99999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Cliente não encontrado", body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
