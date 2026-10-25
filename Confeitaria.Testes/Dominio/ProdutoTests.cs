using Confeitaria.Api.Modelos;
using Xunit;

namespace Confeitaria.Testes.Dominio;

public class ProdutoTests
{
    [Fact]
    public void Criar_ComDadosValidos_PreencheOsCampos()
    {
        var produto = new Produto("  Bolo de Cenoura  ", "Com cobertura de chocolate", 65m, "Bolo");

        Assert.Equal("Bolo de Cenoura", produto.Nome);
        Assert.Equal(65m, produto.Preco);
        Assert.True(produto.Ativo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComPrecoInvalido_Lanca(decimal preco)
    {
        Assert.Throws<ArgumentException>(() => new Produto("Brigadeiro", "", preco, "Doce"));
    }

    [Fact]
    public void Criar_SemNome_Lanca()
    {
        Assert.Throws<ArgumentException>(() => new Produto(" ", "", 10m, "Doce"));
    }

    [Fact]
    public void Atualizar_ComDadosValidos_AlteraOProduto()
    {
        var produto = new Produto("Brigadeiro", "", 3m, "Doce");

        produto.Atualizar("Brigadeiro Gourmet", "Belga", 5m, "Doce", false);

        Assert.Equal("Brigadeiro Gourmet", produto.Nome);
        Assert.Equal(5m, produto.Preco);
        Assert.False(produto.Ativo);
    }
}