using ApiCaixaInvest.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Infrastructure.Data;

public class ApiCaixaInvestDbContext : DbContext
{
    public ApiCaixaInvestDbContext(DbContextOptions<ApiCaixaInvestDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProdutoInvestimento> ProdutosInvestimento => Set<ProdutoInvestimento>();
    public DbSet<SimulacaoInvestimento> Simulacoes => Set<SimulacaoInvestimento>();
    public DbSet<InvestimentoHistorico> InvestimentosHistorico => Set<InvestimentoHistorico>();
    public DbSet<PerfilCliente> PerfisClientes => Set<PerfilCliente>();
    public DbSet<TelemetriaRegistro> TelemetriaRegistros => Set<TelemetriaRegistro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed de produtos de exemplo (facilita testar)
        modelBuilder.Entity<ProdutoInvestimento>().HasData(
            new ProdutoInvestimento { Id = 101, Nome = "CDB Caixa 2026", Tipo = "CDB", RentabilidadeAnual = 0.12m, Risco = "Baixo", PrazoMinimoMeses = 12, LiquidezDias = 30 },
            new ProdutoInvestimento { Id = 102, Nome = "Fundo XPTO", Tipo = "Fundo", RentabilidadeAnual = 0.18m, Risco = "Alto", PrazoMinimoMeses = 6, LiquidezDias = 60 }
        );

        modelBuilder.Entity<InvestimentoHistorico>().HasData(
            new InvestimentoHistorico { Id = 1, ClienteId = 123, Tipo = "CDB", Valor = 5000m, Rentabilidade = 0.12m, Data = new DateTime(2025, 1, 15) },
            new InvestimentoHistorico { Id = 2, ClienteId = 123, Tipo = "Fundo Multimercado", Valor = 3000m, Rentabilidade = 0.08m, Data = new DateTime(2025, 3, 10) }
        );
    }
}
