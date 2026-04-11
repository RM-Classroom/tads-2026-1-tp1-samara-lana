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

            veiculo.Disponivel = false;

            _context.Alugueis.Add(aluguel);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Aluguel criado com sucesso.", aluguel.Id });
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
    }
}