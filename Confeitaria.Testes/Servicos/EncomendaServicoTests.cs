using Confeitaria.Api.Dados;
using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Confeitaria.Api.Servicos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Confeitaria.Testes.Servicos;

public class EncomendaServicoTests : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly AppDbContext _db;
    private readonly EncomendaServico _servico;
    private readonly Usuario _ana;
    private readonly Usuario _bruno;
    private readonly Usuario _admin;
    private readonly Produto _bolo;

    // o xUnit cria uma instância por teste: cada teste ganha um banco novo
    public EncomendaServicoTests()
    {
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options;
        _db = new AppDbContext(opcoes);
        _db.Database.EnsureCreated();

        _ana = new Usuario("Ana Souza", "ana@exemplo.com", "hash", Usuario.PapelCliente);
        _bruno = new Usuario("Bruno Lima", "bruno@exemplo.com", "hash", Usuario.PapelCliente);
        _admin = new Usuario("Administradora", "admin@exemplo.com", "hash", Usuario.PapelAdmin);
        _bolo = new Produto("Bolo de Cenoura", "Com cobertura de chocolate", 68m, "Bolo");

        _db.Usuarios.AddRange(_ana, _bruno, _admin);
        _db.Produtos.Add(_bolo);
        _db.SaveChanges();

        _servico = new EncomendaServico(_db, NullLogger<EncomendaServico>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conexao.Dispose();
    }

    // 15h no horário de Brasília, daqui a N dias
    private static DateTimeOffset Dia(int diasAFrente)
    {
        var data = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(diasAFrente).Date.AddHours(15), DateTimeKind.Unspecified);
        return new DateTimeOffset(data, TimeSpan.FromHours(-3));
    }

    private Task<Resultado<EncomendaDto>> Pedir(Usuario usuario, DateTimeOffset retirada, int quantidade = 1, int? produtoId = null)
        => _servico.CriarAsync(usuario.Id, new EncomendaEntradaDto
        {
            ProdutoId = produtoId ?? _bolo.Id,
            Quantidade = quantidade,
            DataRetirada = retirada
        });

    // ---------- criar e a regra do 409 ----------

    [Fact]
    public async Task Criar_ComDadosValidos_CalculaOValorTotal()
    {
        var resultado = await Pedir(_ana, Dia(30), quantidade: 2);

        Assert.True(resultado.Sucesso);
        Assert.Equal(136m, resultado.Valor!.ValorTotal);
        Assert.Equal("Pendente", resultado.Valor.Status);
        Assert.Equal(_ana.Id, resultado.Valor.UsuarioId);
    }

    [Fact]
    public async Task Criar_QuandoOLimiteDoDiaFoiAtingido_RetornaConflito()
    {
        for (var i = 0; i < EncomendaServico.LimitePorDia; i++)
            Assert.True((await Pedir(_ana, Dia(30))).Sucesso);

        var recusada = await Pedir(_bruno, Dia(30));

        Assert.False(recusada.Sucesso);
        Assert.Equal(TipoErro.Conflito, recusada.Erro);
        Assert.Contains("limite", recusada.Mensagem);
    }

    [Fact]
    public async Task Criar_EmOutroDia_NaoEhAfetadoPeloLimite()
    {
        for (var i = 0; i < EncomendaServico.LimitePorDia; i++)
            await Pedir(_ana, Dia(30));

        var outroDia = await Pedir(_bruno, Dia(31));

        Assert.True(outroDia.Sucesso);
    }

    [Fact]
    public async Task Criar_AposCancelarUma_LiberaAVaga()
    {
        var primeira = await Pedir(_ana, Dia(30));
        for (var i = 1; i < EncomendaServico.LimitePorDia; i++)
            await Pedir(_ana, Dia(30));

        Assert.Equal(TipoErro.Conflito, (await Pedir(_bruno, Dia(30))).Erro);

        await _servico.CancelarAsync(primeira.Valor!.Id, _ana.Id, admin: false);

        Assert.True((await Pedir(_bruno, Dia(30))).Sucesso);
    }

    [Fact]
    public async Task Criar_ComProdutoInexistente_RetornaNaoEncontrado()
    {
        var resultado = await Pedir(_ana, Dia(30), produtoId: 9999);

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Erro);
    }

    [Fact]
    public async Task Criar_ComProdutoInativo_RetornaValidacao()
    {
        var inativo = new Produto("Torta Antiga", "", 50m, "Torta", ativo: false);
        _db.Produtos.Add(inativo);
        await _db.SaveChangesAsync();

        var resultado = await Pedir(_ana, Dia(30), produtoId: inativo.Id);

        Assert.Equal(TipoErro.Validacao, resultado.Erro);
    }

    [Fact]
    public async Task Criar_ComDataNoPassado_RetornaValidacao()
    {
        var resultado = await Pedir(_ana, Dia(-3));

        Assert.Equal(TipoErro.Validacao, resultado.Erro);
    }

    // ---------- autorização: dono ou admin ----------

    [Fact]
    public async Task Cancelar_PorOutroCliente_RetornaProibidoENaoAltera()
    {
        var criada = await Pedir(_ana, Dia(30));

        var resultado = await _servico.CancelarAsync(criada.Valor!.Id, _bruno.Id, admin: false);

        Assert.Equal(TipoErro.Proibido, resultado.Erro);
        var noBanco = await _db.Encomendas.AsNoTracking().SingleAsync(e => e.Id == criada.Valor.Id);
        Assert.Equal(StatusEncomenda.Pendente, noBanco.Status);
    }

    [Fact]
    public async Task Cancelar_PeloDono_Funciona()
    {
        var criada = await Pedir(_ana, Dia(30));

        var resultado = await _servico.CancelarAsync(criada.Valor!.Id, _ana.Id, admin: false);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Cancelada", resultado.Valor!.Status);
    }

    [Fact]
    public async Task Cancelar_PeloAdmin_FuncionaMesmoSendoDeOutro()
    {
        var criada = await Pedir(_ana, Dia(30));

        var resultado = await _servico.CancelarAsync(criada.Valor!.Id, _admin.Id, admin: true);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task Cancelar_DuasVezes_RetornaConflito()
    {
        var criada = await Pedir(_ana, Dia(30));
        await _servico.CancelarAsync(criada.Valor!.Id, _ana.Id, admin: false);

        var segunda = await _servico.CancelarAsync(criada.Valor.Id, _ana.Id, admin: false);

        Assert.Equal(TipoErro.Conflito, segunda.Erro);
    }

    [Fact]
    public async Task Cancelar_Inexistente_RetornaNaoEncontrado()
    {
        var resultado = await _servico.CancelarAsync(9999, _ana.Id, admin: false);

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Erro);
    }

    // ---------- listagem filtrada pelo dono ----------

    [Fact]
    public async Task Listar_Cliente_VeSoAsPropriasEncomendas()
    {
        await Pedir(_ana, Dia(30));
        await Pedir(_bruno, Dia(31));

        var pagina = await _servico.ListarAsync(_ana.Id, admin: false, pagina: 1, tamanho: 10);

        Assert.Single(pagina.Itens);
        Assert.All(pagina.Itens, e => Assert.Equal(_ana.Id, e.UsuarioId));
    }

    [Fact]
    public async Task Listar_Admin_VeTodasAsEncomendas()
    {
        await Pedir(_ana, Dia(30));
        await Pedir(_bruno, Dia(31));

        var pagina = await _servico.ListarAsync(_admin.Id, admin: true, pagina: 1, tamanho: 10);

        Assert.Equal(2, pagina.Total);
    }
}