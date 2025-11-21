using ApiCaixaInvest.Application.Dtos.Requests.Auth;
using ApiCaixaInvest.Application.Dtos.Responses.Auth;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ApiCaixaInvest.Tests.Controllers
{
    public class AuthControllerTests
    {
        #region Fakes

        private class FakeAuthService : IAuthService
        {
            private readonly LoginResponse _response;
            private readonly bool _returnNull;

            public FakeAuthService(LoginResponse response, bool returnNull = false)
            {
                _response = response;
                _returnNull = returnNull;
            }

            public Task<LoginResponse?> AutenticarAsync(LoginRequest request)
            {
                if (_returnNull)
                    return Task.FromResult<LoginResponse?>(null);

                return Task.FromResult<LoginResponse?>(_response);
            }

            // 👇 NOVO: implementação fake do refresh token
            public Task<LoginResponse?> RenovarTokenAsync(RefreshTokenRequest request)
            {
                if (_returnNull)
                    return Task.FromResult<LoginResponse?>(null);

                // pros testes atuais não importa o conteúdo, então
                // podemos reutilizar a mesma resposta fake
                return Task.FromResult<LoginResponse?>(_response);
            }
        }

        #endregion

        [Fact]
        public async Task Login_DeveRetornarBadRequest_QuandoModelStateInvalido()
        {
            // Arrange
            var fakeService = new FakeAuthService(new LoginResponse());
            var controller = new AuthController(fakeService);
            controller.ModelState.AddModelError("Email", "Obrigatório");

            var request = new LoginRequest
            {
                Email = "",
                Senha = "teste"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task Login_DeveRetornarUnauthorized_QuandoCredenciaisInvalidas()
        {
            // Arrange
            var fakeService = new FakeAuthService(new LoginResponse(), returnNull: true);
            var controller = new AuthController(fakeService);

            var request = new LoginRequest
            {
                Email = "usuario@teste.com",
                Senha = "errada"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.NotNull(unauthorized.Value);
        }

        [Fact]
        public async Task Login_DeveRetornarOk_QuandoCredenciaisValidas()
        {
            // Arrange
            var expectedResponse = new LoginResponse
            {
                Token = "token-jwt",
                Email = "caixaverso@caixa.gov.br",
                Perfil = "Usuario"
            };

            var fakeService = new FakeAuthService(expectedResponse);
            var controller = new AuthController(fakeService);

            var request = new LoginRequest
            {
                Email = "caixaverso@caixa.gov.br",
                Senha = "Caixaverso@2025"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<LoginResponse>(ok.Value);
            Assert.Equal(expectedResponse.Token, value.Token);
            Assert.Equal(expectedResponse.Email, value.Email);
            Assert.Equal(expectedResponse.Perfil, value.Perfil);
        }

        [Fact]
        public void Me_DeveRetornarOk_ComUsuarioEPerfil()
        {
            // Arrange
            var fakeService = new FakeAuthService(new LoginResponse());
            var controller = new AuthController(fakeService);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "user@teste.com"),
                new Claim("perfil", "Usuario")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var actionResult = controller.Me();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.NotNull(ok.Value);

            var tipo = ok.Value.GetType();
            var usuarioProp = tipo.GetProperty("Usuario");
            var perfilProp = tipo.GetProperty("Perfil");

            Assert.NotNull(usuarioProp);
            Assert.NotNull(perfilProp);
            Assert.Equal("user@teste.com", usuarioProp.GetValue(ok.Value)?.ToString());
            Assert.Equal("Usuario", perfilProp.GetValue(ok.Value)?.ToString());
        }
    }
}
