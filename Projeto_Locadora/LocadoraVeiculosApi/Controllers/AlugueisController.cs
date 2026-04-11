using LocadoraVeiculosApi.Data;
using LocadoraVeiculosApi.DTOs;
using LocadoraVeiculosApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlugueisController : ControllerBase
    {
        private readonly LocadoraContext _context;

        public AlugueisController(LocadoraContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var alugueis = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                    .ThenInclude(v => v!.Fabricante)
                .Select(a => new
                {
                    a.Id,
                    Cliente = a.Cliente!.Nome,
                    Veiculo = a.Veiculo!.Modelo,
                    Fabricante = a.Veiculo!.Fabricante!.Nome,
                    a.DataInicio,
                    a.DataFimPrevista,
                    a.DataDevolucao,
                    a.QuilometragemInicial,
                    a.QuilometragemFinal,
                    a.ValorDiaria,
                    a.ValorTotal,
                    a.Status
                })
                .ToListAsync();

            return Ok(alugueis);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                    .ThenInclude(v => v!.Fabricante)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    Cliente = a.Cliente!.Nome,
                    Veiculo = a.Veiculo!.Modelo,
                    Fabricante = a.Veiculo!.Fabricante!.Nome,
                    a.DataInicio,
                    a.DataFimPrevista,
                    a.DataDevolucao,
                    a.QuilometragemInicial,
                    a.QuilometragemFinal,
                    a.ValorDiaria,
                    a.ValorTotal,
                    a.Status
                })
                .FirstOrDefaultAsync();

            if (aluguel == null)
                return NotFound(new { mensagem = "Aluguel não encontrado." });

            return Ok(aluguel);
        }

        [HttpGet("filtros/alugueis-por-cliente/{clienteId}")]
        public async Task<ActionResult> GetAlugueisPorCliente(int clienteId)
        {
            var resultado = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Where(a => a.ClienteId == clienteId)
                .Select(a => new
                {
                    Cliente = a.Cliente!.Nome,
                    Veiculo = a.Veiculo!.Modelo,
                    a.DataInicio,
                    a.DataFimPrevista,
                    a.DataDevolucao,
                    a.ValorTotal,
                    a.Status
                })
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("filtros/alugueis-em-aberto")]
        public async Task<ActionResult> GetAlugueisEmAberto()
        {
            var resultado = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                    .ThenInclude(v => v!.Fabricante)
                .Where(a => a.Status == StatusAluguel.Aberto)
                .Select(a => new
                {
                    a.Id,
                    Cliente = a.Cliente!.Nome,
                    Veiculo = a.Veiculo!.Modelo,
                    Fabricante = a.Veiculo!.Fabricante!.Nome,
                    a.DataInicio,
                    a.DataFimPrevista,
                    a.ValorDiaria
                })
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Aluguel aluguel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (aluguel.DataFimPrevista <= aluguel.DataInicio)
                return BadRequest(new { mensagem = "A data final prevista deve ser maior que a data inicial." });

            var cliente = await _context.Clientes.FindAsync(aluguel.ClienteId);
            if (cliente == null)
                return BadRequest(new { mensagem = "Cliente não encontrado." });

            var veiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);
            if (veiculo == null)
                return BadRequest(new { mensagem = "Veículo não encontrado." });

            if (!veiculo.Disponivel)
                return BadRequest(new { mensagem = "Veículo indisponível para aluguel." });

            aluguel.QuilometragemInicial = veiculo.QuilometragemAtual;
            aluguel.ValorDiaria = veiculo.ValorDiariaBase;
            aluguel.Status = StatusAluguel.Aberto;
            aluguel.ValorTotal = null;
            aluguel.QuilometragemFinal = null;
            aluguel.DataDevolucao = null;

            veiculo.Disponivel = false;

            _context.Alugueis.Add(aluguel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = aluguel.Id }, aluguel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Aluguel aluguel)
        {
            if (id != aluguel.Id)
                return BadRequest(new { mensagem = "ID inválido." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var aluguelExistente = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguelExistente == null)
                return NotFound(new { mensagem = "Aluguel não encontrado." });

            if (aluguelExistente.Status != StatusAluguel.Aberto)
                return BadRequest(new { mensagem = "Somente aluguéis em aberto podem ser alterados." });

            if (aluguel.DataFimPrevista <= aluguel.DataInicio)
                return BadRequest(new { mensagem = "A data final prevista deve ser maior que a data inicial." });

            if (aluguel.ClienteId != aluguelExistente.ClienteId)
            {
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == aluguel.ClienteId);
                if (!clienteExiste)
                    return BadRequest(new { mensagem = "Cliente não encontrado." });

                aluguelExistente.ClienteId = aluguel.ClienteId;
            }

            if (aluguel.VeiculoId != aluguelExistente.VeiculoId)
            {
                var novoVeiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);
                if (novoVeiculo == null)
                    return BadRequest(new { mensagem = "Novo veículo não encontrado." });

                if (!novoVeiculo.Disponivel)
                    return BadRequest(new { mensagem = "Novo veículo indisponível para aluguel." });

                if (aluguelExistente.Veiculo != null)
                    aluguelExistente.Veiculo.Disponivel = true;

                novoVeiculo.Disponivel = false;

                aluguelExistente.VeiculoId = aluguel.VeiculoId;
                aluguelExistente.QuilometragemInicial = novoVeiculo.QuilometragemAtual;
                aluguelExistente.ValorDiaria = novoVeiculo.ValorDiariaBase;
            }

            aluguelExistente.DataInicio = aluguel.DataInicio;
            aluguelExistente.DataFimPrevista = aluguel.DataFimPrevista;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/devolucao")]
        public async Task<IActionResult> RegistrarDevolucao(int id, [FromBody] DevolucaoDto dto)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel == null)
                return NotFound(new { mensagem = "Aluguel não encontrado." });

            if (aluguel.Status != StatusAluguel.Aberto)
                return BadRequest(new { mensagem = "Esse aluguel já foi finalizado ou cancelado." });

            if (dto.DataDevolucao < aluguel.DataInicio)
                return BadRequest(new { mensagem = "Data de devolução inválida." });

            if (dto.QuilometragemFinal < aluguel.QuilometragemInicial)
                return BadRequest(new { mensagem = "Quilometragem final não pode ser menor que a inicial." });

            aluguel.DataDevolucao = dto.DataDevolucao;
            aluguel.QuilometragemFinal = dto.QuilometragemFinal;

            int dias = (int)Math.Ceiling((dto.DataDevolucao - aluguel.DataInicio).TotalDays);
            if (dias <= 0)
                dias = 1;

            aluguel.ValorTotal = dias * aluguel.ValorDiaria;
            aluguel.Status = StatusAluguel.Finalizado;

            aluguel.Veiculo!.QuilometragemAtual = dto.QuilometragemFinal;
            aluguel.Veiculo.Disponivel = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Devolução registrada com sucesso.",
                aluguel.Id,
                aluguel.ValorTotal
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel == null)
                return NotFound(new { mensagem = "Aluguel não encontrado." });

            if (aluguel.Status == StatusAluguel.Aberto && aluguel.Veiculo != null)
                aluguel.Veiculo.Disponivel = true;

            _context.Alugueis.Remove(aluguel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}