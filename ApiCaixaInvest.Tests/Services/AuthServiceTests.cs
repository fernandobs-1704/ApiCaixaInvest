using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Application.Options;
using ApiCaixaInvest.Infrastructure.Services;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ApiCaixaInvest.Tests.Services
{
    public class AuthServiceTests
    {
        // Fake simples só pra satisfazer o AuthService.
        // Não precisa de Redis real nos testes.
        private class FakeTokenStore : ITokenStore
        {
            public Task StoreRefreshTokenAsync(string subject, string refreshToken, System.DateTime expiresAt)
            {
                // Não precisamos armazenar nada de verdade pra esses testes
                return Task.CompletedTask;
            }

            public Task<bool> IsRefreshTokenValidAsync(string subject, string refreshToken)
            {
                // Para esses testes, não usamos refresh token
                return Task.FromResult(true);
            }

            public Task RevokeRefreshTokenAsync(string subject, string refreshToken)
            {
                return Task.CompletedTask;
            }
        }

        private AuthService CriarService()
        {
            var opts = Options.Create(new JwtOptions
            {
                SecretKey = "chave-super-secreta-para-testes-1234567890",
                Issuer = "ApiCaixaInvest.Tests",
                Audience = "ApiCaixaInvest.Tests",
                ExpirationMinutes = 60
            });

            var fakeTokenStore = new FakeTokenStore();

            // 👇 Agora passa também o ITokenStore
            return new AuthService(opts, fakeTokenStore);
        }

        [Fact]
        public async Task AutenticarAsync_DeveRetornarToken_QuandoCredenciaisValidas()
        {
            // Arrange
            var service = CriarService();
            var request = new LoginRequest
            {
                Email = "caixaverso@caixa.gov.br",
                Senha = "Caixaverso@2025"
            };

            // Act
            LoginResponse? result = await service.AutenticarAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result!.Token));
            Assert.Equal(request.Email, result.Email);
            Assert.Equal("Usuario", result.Perfil);

            // Opcional: já que agora tem refresh token
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [Fact]
        public async Task AutenticarAsync_DeveRetornarNull_QuandoSenhaInvalida()
        {
            // Arrange
            var service = CriarService();
            var request = new LoginRequest
            {
                Email = "caixaverso@caixa.gov.br",
                Senha = "SenhaErrada"
            };

            // Act
            var result = await service.AutenticarAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AutenticarAsync_DeveGerarTokenComClaimPerfil()
        {
            // Arrange
            var service = CriarService();
            var request = new LoginRequest
            {
                Email = "caixaverso@caixa.gov.br",
                Senha = "Caixaverso@2025"
            };

            // Act
            var result = await service.AutenticarAsync(request);

            // Assert
            Assert.NotNull(result);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result!.Token);

            var perfilClaim = token.Claims.FirstOrDefault(c => c.Type == "perfil");
            Assert.NotNull(perfilClaim);
            Assert.Equal("Usuario", perfilClaim!.Value);
        }
    }
}
