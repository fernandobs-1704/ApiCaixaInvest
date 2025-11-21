using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Produtos;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class RecomendacoesControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public RecomendacoesControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task GetProdutosRecomendados_Conservador_DeveRetornarSomenteBaixo()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/recomendacoes/produtos/Conservador");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var produtos = await response.Content.ReadFromJsonAsync<IEnumerable<ProdutoRecomendadoResponse>>();
            Assert.NotNull(produtos);
            Assert.True(produtos!.Any());

            Assert.All(produtos!, p =>
                Assert.Equal("baixo", p.Risco.ToLowerInvariant()));
        }

        [Fact]
        public async Task GetProdutosRecomendados_PerfilInvalido_DeveRetornar400()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/recomendacoes/produtos/SuperAgressivo");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Perfil inválido", body, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetRecomendacoesPorCliente_DeveRetornar200ELista()
        {
            var client = await CreateAuthenticatedClientAsync();

            // cria ao menos uma simulação só pra registrar o cliente
            await client.PostAsJsonAsync("/api/simular-investimento", new
            {
                ClienteId = 999,
                Valor = 1000,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            });

            var result = await client.GetAsync("/api/recomendacoes/cliente/999");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var json = await result.Content.ReadAsStringAsync();
            Assert.Contains("recomendacoes", json);
        }

    }
}
