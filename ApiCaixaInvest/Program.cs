using ApiCaixaInvest.Api.Extensions;
using ApiCaixaInvest.Application.Interfaces;
using ApiCaixaInvest.Application.Options;
using ApiCaixaInvest.Infrastructure.Data;
using ApiCaixaInvest.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Controllers da API (padrão ASP.NET Core)
builder.Services.AddControllers();

// Gera metadados dos endpoints para o Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Configuração detalhada do Swagger / OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    // Documento principal da API (v1)
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Investimentos – Perfil de Risco Dinâmico",
        Version = "v1",
        Description = "API desenvolvida para simulação de investimentos, cálculo de perfil de risco e recomendação de produtos financeiros."
    });

    // Mapeamento extra para tipos DateOnly e TimeOnly (quando usados nos modelos/DTOs)
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

    // Inclui comentários XML dos controllers e models no Swagger,
    // permitindo que os resumos (/// <summary>) apareçam na documentação.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // Configuração de segurança do Swagger para uso de JWT (Bearer)
    // Isso habilita o botão "Authorize" na UI do Swagger.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT:"
    });

    // Exige o esquema "Bearer" por padrão em todos os endpoints (pode ser refinado por atributo depois)
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
});

// Configuração do Entity Framework Core com SQLite.
builder.Services.AddDbContext<ApiCaixaInvestDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(cs);
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

// Registro da camada de serviços (Interface -> Implementação)
builder.Services.AddScoped<IInvestmentSimulationService, InvestmentSimulationService>();
builder.Services.AddScoped<IRiskProfileService, RiskProfileService>();
builder.Services.AddScoped<IInvestimentosService, InvestimentosService>();
builder.Services.AddScoped<IProdutosService, ProdutosService>();
builder.Services.AddScoped<ISimulacoesConsultaService, SimulacoesConsultaService>();
builder.Services.AddScoped<ITelemetriaQueryService, TelemetriaQueryService>();
builder.Services.AddScoped<TelemetriaService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseApiTelemetria();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
