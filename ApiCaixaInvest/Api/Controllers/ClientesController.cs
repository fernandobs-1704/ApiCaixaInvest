using ApiCaixaInvest.Domain.Models;
using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCaixaInvest.Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly ApiCaixaInvestDbContext _db;

        public ClientesController(ApiCaixaInvestDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lista os clientes conhecidos pela plataforma.",
            Description = "Clientes são criados automaticamente na primeira simulação realizada para um determinado clienteId. " +
                          "Este endpoint permite inspecionar quais clientes já possuem histórico na base."
        )]
        [SwaggerResponse(StatusCodes.Status200OK,
            "Lista de clientes retornada com sucesso.",
            typeof(IEnumerable<Cliente>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized,
            "Token JWT ausente ou inválido.")]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            var clientes = await _db.Clientes
                .OrderBy(c => c.Id)
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Consulta um cliente específico por identificador.",
            Description = "Retorna os dados básicos de um cliente criado automaticamente a partir das simulações."
        )]
        [SwaggerResponse(StatusCodes.Status200OK,
            "Cliente encontrado com sucesso.",
            typeof(Cliente))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized,
            "Token JWT ausente ou inválido.")]
        [SwaggerResponse(StatusCodes.Status404NotFound,
            "Cliente não encontrado.")]
        public async Task<ActionResult<Cliente>> GetClientePorId(int id)
        {
            if (id <= 0)
                return BadRequest(new { mensagem = "O identificador do cliente deve ser maior que zero." });

            var cliente = await _db.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            return Ok(cliente);
        }
    }
}
