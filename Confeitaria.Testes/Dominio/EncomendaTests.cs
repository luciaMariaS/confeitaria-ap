using Confeitaria.Api.Modelos;
using Xunit;

namespace Confeitaria.Testes.Dominio;

public class EncomendaTests
{
    private static DateTime DataFutura() => DateTime.UtcNow.AddDays(3);

    [Fact]
    public void Criar_ComDadosValidos_ComecaPendente()
    {
        var encomenda = new Encomenda(1, 1, 2, DataFutura(), "Sem granulado");

        Assert.Equal(StatusEncomenda.Pendente, encomenda.Status);
        Assert.Equal(2, encomenda.Quantidade);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComQuantidadeInvalida_Lanca(int quantidade)
    {
        Assert.Throws<ArgumentException>(() => new Encomenda(1, 1, quantidade, DataFutura(), null));
    }

    [Fact]
    public void Criar_ComDataNoPassado_Lanca()
    {
        var ontem = DateTime.UtcNow.AddDays(-1);

        Assert.Throws<ArgumentException>(() => new Encomenda(1, 1, 1, ontem, null));
    }

    [Fact]
    public void Criar_ComDataLocal_Lanca()
    {
        var local = DateTime.Now.AddDays(3);

        Assert.Throws<ArgumentException>(() => new Encomenda(1, 1, 1, local, null));
    }

    [Fact]
    public void Cancelar_MudaOStatus()
    {
        var encomenda = new Encomenda(1, 1, 1, DataFutura(), null);

        encomenda.Cancelar();

        Assert.Equal(StatusEncomenda.Cancelada, encomenda.Status);
    }

    [Fact]
    public void Cancelar_DuasVezes_Lanca()
    {
        var encomenda = new Encomenda(1, 1, 1, DataFutura(), null);
        encomenda.Cancelar();

        Assert.Throws<InvalidOperationException>(() => encomenda.Cancelar());
    }
}