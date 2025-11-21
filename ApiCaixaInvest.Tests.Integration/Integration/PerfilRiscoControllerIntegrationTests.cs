using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        private async Task<int> SimularEContratarAsync(HttpClient client)
        {
            var clienteId = 3456;

            var request = new SimularInvestimentoRequest
            {
                ClienteId = clienteId,
                Valor = 1500m,
                PrazoMeses = 18,
                TipoProduto = "CDB"
            };

            // Usa o endpoint que já simula e contrata
            var simContratarResp = await client.PostAsJsonAsync(
                "/api/simular-e-contratar-investimento",
                request);

            Assert.Equal(HttpStatusCode.OK, simContratarResp.StatusCode);

            return clienteId;
        }

        [Fact]
        public async Task ObterPerfilRiscoBasico_AposSimularEContratar_DeveRetornar200()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();
            var clienteId = await SimularEContratarAsync(client);

            // Act
            var perfilResp = await client.GetAsync($"/api/perfil-risco/{clienteId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, perfilResp.StatusCode);

            var perfil = await perfilResp.Content.ReadFromJsonAsync<PerfilRiscoBasicoResponse>();
            Assert.NotNull(perfil);
            Assert.Equal(clienteId, perfil!.ClienteId);
            Assert.False(string.IsNullOrWhiteSpace(perfil.Perfil));
            Assert.True(perfil.Pontuacao > 0);
            Assert.False(string.IsNullOrWhiteSpace(perfil.Descricao));
        }

        [Fact]
        public async Task ObterPerfilRiscoDetalhado_AposSimularEContratar_DeveRetornar200()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();
            var clienteId = await SimularEContratarAsync(client);

            // Act
            var perfilResp = await client.GetAsync($"/api/perfil-risco/detalhado/{clienteId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, perfilResp.StatusCode);

            var perfil = await perfilResp.Content.ReadFromJsonAsync<PerfilRiscoResponse>();
            Assert.NotNull(perfil);
            Assert.Equal(clienteId, perfil!.ClienteId);
            Assert.False(string.IsNullOrWhiteSpace(perfil.Perfil));
            Assert.True(perfil.Pontuacao > 0);
            Assert.False(string.IsNullOrWhiteSpace(perfil.Descricao));

            // Confere se a parte preditiva está vindo
            Assert.NotNull(perfil.TendenciaPerfis);
            Assert.False(string.IsNullOrWhiteSpace(perfil.ProximoPerfilProvavel));
        }

        [Fact]
        public async Task ObterPerfilRiscoIa_AposSimularEContratar_DeveRetornar200()
        {
            // Arrange
            var client = await CreateAuthenticatedClientAsync();
            var clienteId = await SimularEContratarAsync(client);

            // Act
            var iaResp = await client.GetAsync($"/api/perfil-risco-ia/{clienteId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, iaResp.StatusCode);

            var ia = await iaResp.Content.ReadFromJsonAsync<PerfilRiscoIaResponse>();
            Assert.NotNull(ia);
            Assert.Equal(clienteId, ia!.ClienteId);
            Assert.False(string.IsNullOrWhiteSpace(ia.Perfil));
            Assert.True(ia.Pontuacao > 0);

            // Campos de narrativa IA não podem vir vazios
            Assert.False(string.IsNullOrWhiteSpace(ia.Resumo));
            Assert.False(string.IsNullOrWhiteSpace(ia.VisaoComportamentoInvestidor));
            Assert.False(string.IsNullOrWhiteSpace(ia.SugestoesEstrategicas));
            Assert.False(string.IsNullOrWhiteSpace(ia.AcoesRecomendadas));
            Assert.False(string.IsNullOrWhiteSpace(ia.AlertasImportantes));
        }
    }
}
