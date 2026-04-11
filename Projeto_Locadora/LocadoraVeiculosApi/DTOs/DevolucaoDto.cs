using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculosApi.DTOs
{
    public class DevolucaoDto
    {
        [Required]
        public DateTime DataDevolucao { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal QuilometragemFinal { get; set; }
    }
}