using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Requests.Investimentos;
using ApiCaixaInvest.Application.Dtos.Requests.Simulacoes;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Investimentos;
using ApiCaixaInvest.Application.Dtos.Responses.PerfilRisco;
using ApiCaixaInvest.Application.Dtos.Responses.Simulacoes;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace ApiCaixaInvest.Tests.Integration
{
    public class InvestimentosControllerIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public InvestimentosControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task GetInvestimentos_SemInvestimentos_DeveRetornarListaVazia()
        {
            var client = await CreateAuthenticatedClientAsync();
            var clienteId = 999999001;

            var response = await client.GetAsync($"/api/investimentos/{clienteId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var lista = await response.Content.ReadFromJsonAsync<IEnumerable<InvestimentoHistoricoResponse>>();
            Assert.NotNull(lista);
            Assert.Empty(lista!);
        }

        [Fact]
        public async Task EfetivarSimulacoes_DeveCriarInvestimentosEAtualizarPerfil()
        {
            var client = await CreateAuthenticatedClientAsync();
            var clienteId = 2345;

            // 1) Cria duas simulações
            var simReq1 = new SimularInvestimentoRequest
            {
                ClienteId = clienteId,
                Valor = 1000m,
                PrazoMeses = 12,
                TipoProduto = "CDB"
            };

            var simReq2 = new SimularInvestimentoRequest
            {
                ClienteId = clienteId,
                Valor = 2000m,
                PrazoMeses = 24,
                TipoProduto = "CDB"
            };

            var simResp1 = await client.PostAsJsonAsync("/api/simular-investimento", simReq1);
            var simResp2 = await client.PostAsJsonAsync("/api/simular-investimento", simReq2);

            Assert.Equal(HttpStatusCode.OK, simResp1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, simResp2.StatusCode);

            var sim1 = await simResp1.Content.ReadFromJsonAsync<SimularInvestimentoResponse>();
            var sim2 = await simResp2.Content.ReadFromJsonAsync<SimularInvestimentoResponse>();

            Assert.NotNull(sim1);
            Assert.NotNull(sim2);

            // 2) Efetiva as simulações
            var efetivarRequest = new EfetivarSimulacoesRequest
            {
                ClienteId = clienteId,
                SimulacaoIds = new List<int> { sim1!.SimulacaoId, sim2!.SimulacaoId }
            };

            var efetivarResp = await client.PostAsJsonAsync("/api/investimentos/efetivar", efetivarRequest);
            Assert.Equal(HttpStatusCode.OK, efetivarResp.StatusCode);

            var resultado = await efetivarResp.Content.ReadFromJsonAsync<EfetivarSimulacoesResultadoResponse>();
            Assert.NotNull(resultado);
            Assert.True(resultado!.Sucesso);
            Assert.Equal(clienteId, resultado.ClienteId);
            Assert.NotNull(resultado.PerfilRisco);
            Assert.NotEmpty(resultado.SimulacoesEfetivadas);

            // 3) Verifica se investimentos foram criados
            var getInvResp = await client.GetAsync($"/api/investimentos/{clienteId}");
            Assert.Equal(HttpStatusCode.OK, getInvResp.StatusCode);

            var investimentos = await getInvResp.Content.ReadFromJsonAsync<IEnumerable<InvestimentoHistoricoResponse>>();
            Assert.NotNull(investimentos);
            Assert.True(investimentos!.Any());
        }
    }
}
