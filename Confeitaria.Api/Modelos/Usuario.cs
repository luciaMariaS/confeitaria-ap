namespace Confeitaria.Api.Modelos;

public class Usuario
{
    public const string PapelAdmin = "admin";
    public const string PapelCliente = "cliente";

    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Papel { get; private set; } = PapelCliente;

    private Usuario() { } // exigido pelo EF Core

    public Usuario(string nome, string email, string senhaHash, string papel)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("E-mail inválido.", nameof(email));
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(senhaHash));
        if (papel != PapelAdmin && papel != PapelCliente)
            throw new ArgumentException("Papel inválido.", nameof(papel));

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        Papel = papel;
    }
}