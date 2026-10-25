namespace Confeitaria.Api.Modelos;

public class Produto
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public string Categoria { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    public List<Encomenda> Encomendas { get; private set; } = new();

    private Produto() { }

    public Produto(string nome, string descricao, decimal preco, string categoria, bool ativo = true)
    {
        Atualizar(nome, descricao, preco, categoria, ativo);
    }

    public void Atualizar(string nome, string descricao, decimal preco, string categoria, bool ativo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        if (preco <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.", nameof(preco));
        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("A categoria é obrigatória.", nameof(categoria));

        Nome = nome.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        Preco = preco;
        Categoria = categoria.Trim();
        Ativo = ativo;
    }
}