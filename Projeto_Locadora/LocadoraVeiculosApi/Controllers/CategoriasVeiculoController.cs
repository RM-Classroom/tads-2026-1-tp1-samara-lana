using LocadoraVeiculosApi.Data;
using LocadoraVeiculosApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasVeiculoController : ControllerBase
    {
        private readonly LocadoraContext _context;

        public CategoriasVeiculoController(LocadoraContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaVeiculo>>> GetAll()
        {
            return Ok(await _context.CategoriasVeiculo.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaVeiculo>> GetById(int id)
        {
            var categoria = await _context.CategoriasVeiculo.FindAsync(id);

            if (categoria == null)
                return NotFound(new { mensagem = "Categoria não encontrada." });

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaVeiculo>> Create(CategoriaVeiculo categoria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.CategoriasVeiculo.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoriaVeiculo categoria)
        {
            if (id != categoria.Id)
                return BadRequest(new { mensagem = "ID inválido." });

            var existente = await _context.CategoriasVeiculo.FindAsync(id);

            if (existente == null)
                return NotFound(new { mensagem = "Categoria não encontrada." });

            existente.Nome = categoria.Nome;
            existente.Descricao = categoria.Descricao;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.CategoriasVeiculo.FindAsync(id);

            if (categoria == null)
                return NotFound(new { mensagem = "Categoria não encontrada." });

            _context.CategoriasVeiculo.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}