using LocadoraVeiculosApi.Data;
using LocadoraVeiculosApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeiculosController : ControllerBase
    {
        private readonly LocadoraContext _context;

        public VeiculosController(LocadoraContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var veiculos = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Include(v => v.CategoriaVeiculo)
                .Select(v => new
                {
                    v.Id,
                    v.Modelo,
                    v.AnoFabricacao,
                    v.QuilometragemAtual,
                    v.Placa,
                    v.Cor,
                    v.ValorDiariaBase,
                    v.Disponivel,
                    Fabricante = v.Fabricante!.Nome,
                    Categoria = v.CategoriaVeiculo!.Nome
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Include(v => v.CategoriaVeiculo)
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    v.Id,
                    v.Modelo,
                    v.AnoFabricacao,
                    v.QuilometragemAtual,
                    v.Placa,
                    v.Cor,
                    v.ValorDiariaBase,
                    v.Disponivel,
                    Fabricante = v.Fabricante!.Nome,
                    Categoria = v.CategoriaVeiculo!.Nome
                })
                .FirstOrDefaultAsync();

            if (veiculo == null)
                return NotFound(new { mensagem = "Veículo não encontrado." });

            return Ok(veiculo);
        }

        [HttpPost]
        public async Task<ActionResult<Veiculo>> Create(Veiculo veiculo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool placaExiste = await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa);
            if (placaExiste)
                return BadRequest(new { mensagem = "Placa já cadastrada." });

            bool fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId);
            bool categoriaExiste = await _context.CategoriasVeiculo.AnyAsync(c => c.Id == veiculo.CategoriaVeiculoId);

            if (!fabricanteExiste || !categoriaExiste)
                return BadRequest(new { mensagem = "Fabricante ou categoria inválidos." });

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = veiculo.Id }, veiculo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Veiculo veiculo)
        {
            if (id != veiculo.Id)
                return BadRequest(new { mensagem = "ID inválido." });

            var existente = await _context.Veiculos.FindAsync(id);

            if (existente == null)
                return NotFound(new { mensagem = "Veículo não encontrado." });

            existente.Modelo = veiculo.Modelo;
            existente.AnoFabricacao = veiculo.AnoFabricacao;
            existente.QuilometragemAtual = veiculo.QuilometragemAtual;
            existente.Placa = veiculo.Placa;
            existente.Cor = veiculo.Cor;
            existente.ValorDiariaBase = veiculo.ValorDiariaBase;
            existente.Disponivel = veiculo.Disponivel;
            existente.FabricanteId = veiculo.FabricanteId;
            existente.CategoriaVeiculoId = veiculo.CategoriaVeiculoId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo == null)
                return NotFound(new { mensagem = "Veículo não encontrado." });

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("filtros/veiculos-por-fabricante/{fabricanteNome}")]
        public async Task<ActionResult> GetVeiculosPorFabricante(string fabricanteNome)
        {
            var resultado = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Where(v => v.Fabricante!.Nome.Contains(fabricanteNome))
                .Select(v => new
                {
                    v.Modelo,
                    v.Placa,
                    v.AnoFabricacao,
                    Fabricante = v.Fabricante!.Nome
                })
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("filtros/veiculos-por-categoria/{categoriaNome}")]
        public async Task<ActionResult> GetVeiculosPorCategoria(string categoriaNome)
        {
            var resultado = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .Where(v => v.CategoriaVeiculo!.Nome.Contains(categoriaNome))
                .Select(v => new
                {
                    v.Modelo,
                    v.Placa,
                    Categoria = v.CategoriaVeiculo!.Nome,
                    v.ValorDiariaBase
                })
                .ToListAsync();

            return Ok(resultado);
        }
    }
}