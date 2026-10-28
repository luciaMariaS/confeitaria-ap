using System.ComponentModel.DataAnnotations;

namespace Confeitaria.Api.Dtos;

public class EncomendaEntradaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe um produto válido.")]
    public int ProdutoId { get; set; }

    [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")]
    public int Quantidade { get; set; }

    [Required]
    public DateTimeOffset DataRetirada { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

public class EncomendaDto
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTimeOffset DataRetirada { get; set; }
    public string Observacoes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; set; }
}