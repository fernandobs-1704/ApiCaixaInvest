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

        // Relacionamento simples entre InvestimentoHistorico e ProdutoInvestimento
        modelBuilder.Entity<InvestimentoHistorico>()
            .HasOne(i => i.ProdutoInvestimento)
            .WithMany()
            .HasForeignKey(i => i.ProdutoInvestimentoId);

        modelBuilder.Entity<SimulacaoInvestimento>()
            .HasOne(s => s.ProdutoInvestimento)
            .WithMany()
            .HasForeignKey(s => s.ProdutoInvestimentoId);

        // Seed de produtos clássicos para o desafio
        modelBuilder.Entity<ProdutoInvestimento>().HasData(
            // -----------------------------
            //  BAIXO RISCO  (3 produtos)
            // -----------------------------
            new ProdutoInvestimento
            {
                Id = 101,
                Nome = "CDB Caixa Liquidez Diária",
                Tipo = "CDB",
                RentabilidadeAnual = 0.105m,
                Risco = "Baixo",
                PrazoMinimoMeses = 6,
                LiquidezDias = 1
            },
            new ProdutoInvestimento
            {
                Id = 102,
                Nome = "Tesouro Selic 2029",
                Tipo = "Tesouro Direto",
                RentabilidadeAnual = 0.10m,
                Risco = "Baixo",
                PrazoMinimoMeses = 1,
                LiquidezDias = 1
            },
            new ProdutoInvestimento
            {
                Id = 103,
                Nome = "LCI Caixa 1 Ano",
                Tipo = "LCI",
                RentabilidadeAnual = 0.11m,
                Risco = "Baixo",
                PrazoMinimoMeses = 12,
                LiquidezDias = 60
            },

            // -----------------------------
            //  MÉDIO RISCO  (3 produtos)
            // -----------------------------
            new ProdutoInvestimento
            {
                Id = 201,
                Nome = "Tesouro IPCA+ 2035",
                Tipo = "Tesouro Direto",
                RentabilidadeAnual = 0.13m,
                Risco = "Médio",
                PrazoMinimoMeses = 36,
                LiquidezDias = 30
            },
            new ProdutoInvestimento
            {
                Id = 202,
                Nome = "Fundo Renda Fixa Premium",
                Tipo = "Fundo de Renda Fixa",
                RentabilidadeAnual = 0.145m,
                Risco = "Médio",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            },
            new ProdutoInvestimento
            {
                Id = 203,
                Nome = "LCA Caixa 2 Anos",
                Tipo = "LCA",
                RentabilidadeAnual = 0.12m,
                Risco = "Médio",
                PrazoMinimoMeses = 24,
                LiquidezDias = 180
            },

            // -----------------------------
            //  ALTO RISCO  (3 produtos)
            // -----------------------------
            new ProdutoInvestimento
            {
                Id = 301,
                Nome = "Fundo Multimercado XPTO",
                Tipo = "Fundo Multimercado",
                RentabilidadeAnual = 0.18m,
                Risco = "Alto",
                PrazoMinimoMeses = 6,
                LiquidezDias = 30
            },
            new ProdutoInvestimento
            {
                Id = 302,
                Nome = "Fundo de Ações Brasil",
                Tipo = "Fundo de Ações",
                RentabilidadeAnual = 0.22m,
                Risco = "Alto",
                PrazoMinimoMeses = 12,
                LiquidezDias = 30
            },
            new ProdutoInvestimento
            {
                Id = 303,
                Nome = "ETF BOVA11",
                Tipo = "Ações/ETF",
                RentabilidadeAnual = 0.25m,
                Risco = "Alto",
                PrazoMinimoMeses = 1,
                LiquidezDias = 3
            }
        );
    }
}
