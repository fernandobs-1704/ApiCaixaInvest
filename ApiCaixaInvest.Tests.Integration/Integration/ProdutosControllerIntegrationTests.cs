using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Domain.Models;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class ProdutosControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ProdutosControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task GetProdutos_DeveRetornarListaComItens()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/produtos");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var produtos = await response.Content.ReadFromJsonAsync<IEnumerable<ProdutoInvestimento>>();
            Assert.NotNull(produtos);
            Assert.True(produtos!.Any());
        }

        [Fact]
        public async Task GetProdutoPorId_Existente_DeveRetornar200()
        {
            var client = await CreateAuthenticatedClientAsync();

            // Usa um ID conhecido do seed
            var response = await client.GetAsync("/api/produtos/101");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var produto = await response.Content.ReadFromJsonAsync<ProdutoInvestimento>();
            Assert.NotNull(produto);
            Assert.Equal(101, produto!.Id);
        }

        [Fact]
        public async Task GetProdutosPorRisco_Baixo_DeveRetornarSomenteBaixo()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/produtos/risco/Baixo");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var produtos = await response.Content.ReadFromJsonAsync<IEnumerable<ProdutoInvestimento>>();
            Assert.NotNull(produtos);
            Assert.True(produtos!.Any());

            Assert.All(produtos!, p =>
                Assert.Equal("baixo", p.Risco.ToLowerInvariant()));
        }
    }
}
