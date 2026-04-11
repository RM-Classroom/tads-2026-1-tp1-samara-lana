using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculosApi.Models
{
    public class CategoriaVeiculo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descricao { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }

}

