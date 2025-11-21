using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class SimulacoesControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SimulacoesControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient()
        {
            return _factory.CreateClient();
        }

        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = CreateClient();
            await AutenticarAsync(client);
            return client;
        }


        private async Task AutenticarAsync(HttpClient client)
        {
            var request = new LoginRequest
            {
                Email = "caixaverso@caixa.gov.br",
                Senha = "Caixaverso@2025"
            };

            var response = await client.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Falha na autenticação: {response.StatusCode}");
            }

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(body);
            Assert.False(string.IsNullOrWhiteSpace(body!.Token));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", body.Token);
        }

        [Fact]
        public async Task SimularInvestimento_ComDadosValidos_DeveRetornar200EObjeto()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 123,
                Valor = 1000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/simular-investimento", request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<object>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing BadRequest: {ex.Message}");
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.True(false, $"Expected OK but got {response.StatusCode}. Check console for details.");
            }

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<SimularInvestimentoResponse>();
            Assert.NotNull(body);
            Assert.True(body!.SimulacaoId > 0);
            Assert.NotNull(body.ProdutoValidado);
            Assert.NotNull(body.ResultadoSimulacao);

            Assert.Equal("CDB", body.ProdutoValidado!.Tipo);
            Assert.Equal(request.PrazoMeses, body.ResultadoSimulacao!.PrazoMeses);
        }


        [Fact]
        public async Task SimularInvestimento_ComValorInvalido_DeveRetornar400()
        {
            var client = await CreateAuthenticatedClientAsync();

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 123,
                Valor = 0m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var response = await client.PostAsJsonAsync("/api/simular-investimento", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("maior que zero", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetSimulacoes_SemFiltros_DeveRetornar200()
        {
            var client = await CreateAuthenticatedClientAsync();

            var request = new SimularInvestimentoRequest
            {
                ClienteId = 456,
                Valor = 500m,
                PrazoMeses = 6,
                TipoProduto = "CDB"
            };

            var simResponse = await client.PostAsJsonAsync("/api/simular-investimento", request);
            Assert.Equal(HttpStatusCode.OK, simResponse.StatusCode);

            var response = await client.GetAsync("/api/simulacoes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var lista = await response.Content.ReadFromJsonAsync<IEnumerable<SimulacaoHistoricoResponse>>();
            Assert.NotNull(lista);
            Assert.True(lista!.Any());
        }
    }
}