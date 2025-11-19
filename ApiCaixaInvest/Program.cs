using ApiCaixaInvest.Data;
using ApiCaixaInvest.Helpers;
using ApiCaixaInvest.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

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
        Description = "Insira o token JWT desta forma: Bearer {seu token}"
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

// Registro da camada de serviço de simulação de investimentos.
// AddScoped = uma instância por request HTTP.
builder.Services.AddScoped<InvestmentSimulationService>();
builder.Services.AddScoped<RiskProfileService>();
builder.Services.AddScoped<TelemetriaService>();

var app = builder.Build();

// Garante que o banco de dados exista e aplique o modelo atual.
// Em ambiente real, o ideal é usar Migrations ao invés de EnsureCreated.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiCaixaInvestDbContext>();
    db.Database.EnsureCreated();
}

// Configuração do pipeline de requisições HTTP.

// Em ambiente de desenvolvimento, habilita o Swagger UI.
// Se quiser Swagger também em produção para o desafio,
// pode remover o IF e deixar sempre ativo.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redireciona automaticamente HTTP -> HTTPS (boa prática para APIs públicas)
app.UseHttpsRedirection();

app.UseApiTelemetria();

app.UseAuthorization();

// Mapeia automaticamente os controllers anotados com [ApiController]
app.MapControllers();

// Inicia a aplicação Web
app.Run();
