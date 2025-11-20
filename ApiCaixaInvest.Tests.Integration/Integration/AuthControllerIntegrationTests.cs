using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ApiCaixaInvest.Tests.Integration.Integration;

public class AuthControllerIntegrationTests
     : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        // Cria um HttpClient que chama a API "de verdade" em memória
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornar200EToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "caixaverso@caixa.gov.br",
            Senha = "Caixaverso@2025"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(request.Email, body.Email);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornar401()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "usuario@invalido.com",
            Senha = "senhaerrada"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
