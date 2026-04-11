using LocadoraVeiculosApi.Data;
using LocadoraVeiculosApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly LocadoraContext _context;

        public ClientesController(LocadoraContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
        {
            return Ok(await _context.Clientes.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool cpfExiste = await _context.Clientes.AnyAsync(c => c.CPF == cliente.CPF);
            if (cpfExiste)
                return BadRequest(new { mensagem = "CPF já cadastrado." });

            bool emailExiste = await _context.Clientes.AnyAsync(c => c.Email == cliente.Email);
            if (emailExiste)
                return BadRequest(new { mensagem = "E-mail já cadastrado." });

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Cliente cliente)
        {
            if (id != cliente.Id)
                return BadRequest(new { mensagem = "ID inválido." });

            var existente = await _context.Clientes.FindAsync(id);

            if (existente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            existente.Nome = cliente.Nome;
            existente.CPF = cliente.CPF;
            existente.Email = cliente.Email;
            existente.Telefone = cliente.Telefone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("filtros/clientes-com-alugueis")]
        public async Task<ActionResult> GetClientesComAlugueis()
        {
            var resultado = await _context.Clientes
                .GroupJoin(
                    _context.Alugueis,
                    cliente => cliente.Id,
                    aluguel => aluguel.ClienteId,
                    (cliente, alugueis) => new
                    {
                        Cliente = cliente.Nome,
                        cliente.Email,
                        QuantidadeAlugueis = alugueis.Count()
                    })
                .ToListAsync();

            return Ok(resultado);
        }
    }
}