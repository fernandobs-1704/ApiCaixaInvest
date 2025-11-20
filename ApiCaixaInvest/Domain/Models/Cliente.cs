using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCaixaInvest.Domain.Models;

/// <summary>
/// Representa um cliente da plataforma de investimentos.
/// Criado automaticamente ao executar uma simulação pela primeira vez.
/// </summary>
[NotMapped]
public class Cliente
{
    /// <summary>
    /// Identificador único do cliente (definido manualmente, não gerado pelo banco).
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    /// <summary>
    /// Data de criação do cliente no sistema.
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}
