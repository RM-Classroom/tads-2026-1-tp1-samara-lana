using LocadoraVeiculosApi.Data;
using LocadoraVeiculosApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FabricantesController : ControllerBase
    {
        private readonly LocadoraContext _context;

        public FabricantesController(LocadoraContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fabricante>>> GetAll()
        {
            return Ok(await _context.Fabricantes.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Fabricante>> GetById(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);

            if (fabricante == null)
                return NotFound(new { mensagem = "Fabricante não encontrado." });

            return Ok(fabricante);
        }

        [HttpPost]
        public async Task<ActionResult<Fabricante>> Create(Fabricante fabricante)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Fabricantes.Add(fabricante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = fabricante.Id }, fabricante);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Fabricante fabricante)
        {
            if (id != fabricante.Id)
                return BadRequest(new { mensagem = "ID inválido." });

            var existente = await _context.Fabricantes.FindAsync(id);

            if (existente == null)
                return NotFound(new { mensagem = "Fabricante não encontrado." });

            existente.Nome = fabricante.Nome;
            existente.PaisOrigem = fabricante.PaisOrigem;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);

            if (fabricante == null)
                return NotFound(new { mensagem = "Fabricante não encontrado." });

            _context.Fabricantes.Remove(fabricante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}