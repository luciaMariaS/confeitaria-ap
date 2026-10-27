using Confeitaria.Api.Dados;
using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Confeitaria.Api.Servicos;

public class ProdutoServico
{
    private const int TamanhoMaximo = 50;

    private readonly AppDbContext _db;
    private readonly ILogger<ProdutoServico> _logger;

    public ProdutoServico(AppDbContext db, ILogger<ProdutoServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginaDto<ProdutoDto>> ListarAsync(string? busca, int pagina, int tamanho, bool incluirInativos)
    {
        pagina = Math.Max(pagina, 1);
        tamanho = Math.Clamp(tamanho, 1, TamanhoMaximo);

        var consulta = _db.Produtos.AsNoTracking().AsQueryable();

        if (!incluirInativos)
            consulta = consulta.Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            consulta = consulta.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                p.Descricao.ToLower().Contains(termo) ||
                p.Categoria.ToLower().Contains(termo));
        }

        var total = await consulta.CountAsync();

        var produtos = await consulta
            .OrderBy(p => p.Nome)
            .ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return new PaginaDto<ProdutoDto>
        {
            Itens = produtos.Select(Mapear).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)tamanho)
        };
    }

    public async Task<Resultado<ProdutoDto>> ObterAsync(int id)
    {
        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        return produto is null
            ? Resultado<ProdutoDto>.Falha(TipoErro.NaoEncontrado, $"Produto {id} não encontrado.")
            : Resultado<ProdutoDto>.Ok(Mapear(produto));
    }

    public async Task<Resultado<ProdutoDto>> CriarAsync(ProdutoEntradaDto dto)
    {
        Produto produto;
        try
        {
            produto = new Produto(dto.Nome, dto.Descricao ?? string.Empty, dto.Preco, dto.Categoria, dto.Ativo);
        }
        catch (ArgumentException ex)
        {
            return Resultado<ProdutoDto>.Falha(TipoErro.Validacao, ex.Message);
        }

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Produto {ProdutoId} criado: {Nome}", produto.Id, produto.Nome);
        return Resultado<ProdutoDto>.Ok(Mapear(produto));
    }

    public async Task<Resultado<ProdutoDto>> AtualizarAsync(int id, ProdutoEntradaDto dto)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null)
            return Resultado<ProdutoDto>.Falha(TipoErro.NaoEncontrado, $"Produto {id} não encontrado.");

        try
        {
            produto.Atualizar(dto.Nome, dto.Descricao ?? string.Empty, dto.Preco, dto.Categoria, dto.Ativo);
        }
        catch (ArgumentException ex)
        {
            return Resultado<ProdutoDto>.Falha(TipoErro.Validacao, ex.Message);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Produto {ProdutoId} atualizado", produto.Id);
        return Resultado<ProdutoDto>.Ok(Mapear(produto));
    }

    public async Task<Resultado<bool>> ExcluirAsync(int id)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, $"Produto {id} não encontrado.");

        if (await _db.Encomendas.AnyAsync(e => e.ProdutoId == id))
        {
            _logger.LogWarning("Exclusão do produto {ProdutoId} recusada: há encomendas ligadas", id);
            return Resultado<bool>.Falha(TipoErro.Conflito,
                "Este produto tem encomendas registradas e não pode ser excluído. Desative-o em vez disso.");
        }

        _db.Produtos.Remove(produto);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Produto {ProdutoId} excluído", id);
        return Resultado<bool>.Ok(true);
    }

    private static ProdutoDto Mapear(Produto p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Descricao = p.Descricao,
        Preco = p.Preco,
        Categoria = p.Categoria,
        Ativo = p.Ativo
    };
}