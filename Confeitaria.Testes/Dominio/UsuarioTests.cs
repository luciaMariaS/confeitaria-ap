using Confeitaria.Api.Modelos;
using Xunit;

namespace Confeitaria.Testes.Dominio;

public class UsuarioTests
{
    [Fact]
    public void Criar_NormalizaOEmail()
    {
        var usuario = new Usuario("Ana", "  ANA@Exemplo.com ", "hash", Usuario.PapelCliente);

        Assert.Equal("ana@exemplo.com", usuario.Email);
    }

    [Fact]
    public void Criar_ComEmailSemArroba_Lanca()
    {
        Assert.Throws<ArgumentException>(() => new Usuario("Ana", "anaexemplo.com", "hash", Usuario.PapelCliente));
    }

    [Fact]
    public void Criar_ComPapelInvalido_Lanca()
    {
        Assert.Throws<ArgumentException>(() => new Usuario("Ana", "ana@exemplo.com", "hash", "superadmin"));
    }
}