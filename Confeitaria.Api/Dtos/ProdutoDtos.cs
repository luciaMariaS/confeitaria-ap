using System.ComponentModel.DataAnnotations;

namespace Confeitaria.Api.Dtos;

public class ProdutoEntradaDto
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Range(0.01, 100000)]
    public decimal Preco { get; set; }

    [Required, StringLength(60, MinimumLength = 2)]
    public string Categoria { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}

public class ProdutoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class PaginaDto<T>
{
    public List<T> Itens { get; set; } = new();
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}