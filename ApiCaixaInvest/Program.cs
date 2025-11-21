using ApiCaixaInvest.Api.Extensions;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Application.Options;
using ApiCaixaInvest.Infrastructure.Data;
using ApiCaixaInvest.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Controllers da API (padrão ASP.NET Core)
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Documento principal
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Investimentos – Perfil de Risco Dinâmico",
        Version = "v1",
        Description = "API para simulação de investimentos, cálculo de perfil de risco dinâmico, recomendação de produtos e telemetria.",
        Contact = new OpenApiContact
        {
            Name = "Desafio CaixaVerso",
        }
    });

    // Mapeia DateOnly / TimeOnly
    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Description = "Formato de data: yyyy-MM-dd"
    });

    c.MapType<TimeOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time",
        Description = "Formato de hora: HH:mm:ss"
    });

    // Comentários XML (summary/remarks)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Segurança JWT (botão Authorize)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT neste formato: **Bearer {seu_token}**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Agrupar endpoints por controller (cada controller vira uma TAG colorida)
    c.TagActionsBy(api =>
    {
        // Usa o nome do controller como tag (Produtos, Simulacoes, Investimentos, etc.)
        var controllerName = api.GroupName ?? api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "API" };
    });

    // Mostra o nome dos métodos HTTP nos botões (GET, POST, etc.)
    c.EnableAnnotations();
    c.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => new
            {
                Campo = e.Key,
                Mensagens = e.Value!.Errors.Select(err => err.ErrorMessage)
            });

        var resposta = new
        {
            sucesso = false,
            mensagem = "A requisição contém dados inválidos. Verifique os campos enviados.",
            erros
        };

        return new BadRequestObjectResult(resposta);
    };
});

// Configuração do Entity Framework Core com SQLite.
builder.Services.AddDbContext<ApiCaixaInvestDbContext>((services, options) =>
{
    // Verifica se estamos em ambiente de teste
    var isTestEnvironment = builder.Environment.IsEnvironment("Testing") ||
                           builder.Environment.EnvironmentName == "Test";

    if (isTestEnvironment)
    {
        // Para testes, usa InMemory
        options.UseInMemoryDatabase("InMemoryTestDb");
    }
    else
    {
        // Para desenvolvimento/produção, usa SQLite
        var cs = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlite(cs);
    }
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(configuration))
        throw new InvalidOperationException("ConnectionStrings:Redis não foi configurada.");

    return ConnectionMultiplexer.Connect(configuration);
});

// Registro da camada de serviços (Interface -> Implementação)
builder.Services.AddScoped<IRiskProfileService, PerfilRiscoService>();
builder.Services.AddScoped<IInvestimentosService, InvestimentosService>();
builder.Services.AddScoped<IProdutosService, ProdutosService>();
builder.Services.AddScoped<ITelemetriaService, TelemetriaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ISimulacoesService, SimulacoesService>();

builder.Services.AddScoped<ITokenStore, RedisTokenStore>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection.GetValue<string>("SecretKey");
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30) // tolerância de 30 segundos no momento que expirar
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();

                if (context.Response.HasStarted)
                    return Task.CompletedTask;

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                // Determina se o erro foi token faltando ou token inválido
                var erroPadrao = "TokenInvalido";
                var detalhesPadrao = "O token informado está corrompido, expirado ou não corresponde à assinatura esperada.";

                string mensagem;

                // Quando NÃO existe token algum
                if (string.IsNullOrEmpty(context.Error) && string.IsNullOrEmpty(context.ErrorDescription))
                {
                    mensagem = "Credenciais não enviadas. Informe um token JWT válido no cabeçalho Authorization.";
                    erroPadrao = "TokenNaoInformado";
                    detalhesPadrao = "Nenhum token foi enviado na requisição.";
                }
                else
                {
                    mensagem = "Token inválido ou expirado.";
                    erroPadrao = "TokenInvalido";
                }

                var payload = new
                {
                    sucesso = false,
                    mensagem,
                    erro = erroPadrao,
                    detalhes = detalhesPadrao
                };

                var json = JsonSerializer.Serialize(payload);
                return context.Response.WriteAsync(json);
            }

        };
    });


var app = builder.Build();

// Garante que o banco de dados exista e aplique o modelo atual.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiCaixaInvestDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Investimentos v1");
        options.DocumentTitle = "Painel de Investimentos – Desafio CaixaVerso";
        options.RoutePrefix = "swagger"; // acesso em /swagger

        // UI mais amigável
        options.DisplayRequestDuration(); // mostra tempo de resposta
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // endpoints em lista
    });
}

app.UseHttpsRedirection();

app.UseApiTelemetria();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


namespace ApiCaixaInvest
{
    public partial class Program { }
}
