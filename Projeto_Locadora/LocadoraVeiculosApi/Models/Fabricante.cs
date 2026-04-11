using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculosApi.Models;

public class Fabricante
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(80)]
    public string? PaisOrigem { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}