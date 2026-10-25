namespace Confeitaria.Api.Modelos;

public enum StatusEncomenda
{
    Pendente = 0,
    Cancelada = 1
}

public class Encomenda
{
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public Produto? Produto { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public int Quantidade { get; private set; }
    public DateTime DataRetirada { get; private set; }
    public string Observacoes { get; private set; } = string.Empty;
    public StatusEncomenda Status { get; private set; } = StatusEncomenda.Pendente;
    public DateTime CriadoEm { get; private set; }

    private Encomenda() { }

    public Encomenda(int produtoId, int usuarioId, int quantidade, DateTime dataRetirada, string? observacoes)
    {
        if (produtoId <= 0)
            throw new ArgumentException("Produto inválido.", nameof(produtoId));
        if (usuarioId <= 0)
            throw new ArgumentException("Usuário inválido.", nameof(usuarioId));
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantidade));
        if (dataRetirada.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de retirada deve estar em UTC.", nameof(dataRetirada));
        if (dataRetirada <= DateTime.UtcNow)
            throw new ArgumentException("A data de retirada deve ser futura.", nameof(dataRetirada));

        ProdutoId = produtoId;
        UsuarioId = usuarioId;
        Quantidade = quantidade;
        DataRetirada = dataRetirada;
        Observacoes = observacoes?.Trim() ?? string.Empty;
        CriadoEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusEncomenda.Cancelada)
            throw new InvalidOperationException("A encomenda já está cancelada.");
        Status = StatusEncomenda.Cancelada;
    }
}