using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculosApi.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int AnoFabricacao { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal QuilometragemAtual { get; set; }

        [Required]
        [StringLength(10)]
        public string Placa { get; set; } = string.Empty;

        [StringLength(40)]
        public string? Cor { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal ValorDiariaBase { get; set; }

        public bool Disponivel { get; set; } = true;

        public int FabricanteId { get; set; }
        public Fabricante? Fabricante { get; set; }

        public int CategoriaVeiculoId { get; set; }
        public CategoriaVeiculo? CategoriaVeiculo { get; set; }

        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
