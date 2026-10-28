using System.Data;
using Confeitaria.Api.Dados;
using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Confeitaria.Api.Servicos;

public class EncomendaServico
{
    public const int LimitePorDia = 5;
    private const int TamanhoMaximo = 50;
    private static readonly TimeSpan FusoBrasil = TimeSpan.FromHours(-3);

    private readonly AppDbContext _db;
    private readonly ILogger<EncomendaServico> _logger;

    public EncomendaServico(AppDbContext db, ILogger<EncomendaServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Resultado<EncomendaDto>> CriarAsync(int usuarioId, EncomendaEntradaDto dto)
    {
        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.ProdutoId);
        if (produto is null)
            return Resultado<EncomendaDto>.Falha(TipoErro.NaoEncontrado, $"Produto {dto.ProdutoId} não encontrado.");
        if (!produto.Ativo)
            return Resultado<EncomendaDto>.Falha(TipoErro.Validacao, "Este produto não está disponível para encomenda.");

        Encomenda encomenda;
        try
        {
            encomenda = new Encomenda(produto.Id, usuarioId, dto.Quantidade, dto.DataRetirada.UtcDateTime, dto.Observacoes);
        }
        catch (ArgumentException ex)
        {
            return Resultado<EncomendaDto>.Falha(TipoErro.Validacao, ex.Message);
        }

        // o "dia" da retirada é o dia no horário de Brasília (UTC-3), não em UTC
        var local = dto.DataRetirada.ToOffset(FusoBrasil);
        var inicio = new DateTimeOffset(local.Date, FusoBrasil).UtcDateTime;
        var fim = inicio.AddDays(1);

        await using var transacao = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var ocupadas = await _db.Encomendas.CountAsync(e =>
                e.Status != StatusEncomenda.Cancelada &&
                e.DataRetirada >= inicio && e.DataRetirada < fim);

            if (ocupadas >= LimitePorDia)
            {
                _logger.LogWarning("Encomenda recusada: limite de {Limite} atingido em {Dia}", LimitePorDia, local.Date);
                return Resultado<EncomendaDto>.Falha(TipoErro.Conflito,
                    $"A data {local:dd/MM/yyyy} já atingiu o limite de {LimitePorDia} encomendas. Escolha outra data.");
            }

            _db.Encomendas.Add(encomenda);
            await _db.SaveChangesAsync();
            await transacao.CommitAsync();
        }
        catch (Exception ex) when (ex is DbUpdateException or PostgresException)
        {
            _logger.LogWarning(ex, "Encomenda recusada por concorrência em {Dia}", local.Date);
            return Resultado<EncomendaDto>.Falha(TipoErro.Conflito,
                "Muitas encomendas ao mesmo tempo para essa data. Tente novamente.");
        }

        _logger.LogInformation("Encomenda {EncomendaId} criada pelo usuário {UsuarioId}", encomenda.Id, usuarioId);
        return Resultado<EncomendaDto>.Ok(Mapear(encomenda, produto.Nome, produto.Preco));
    }

    public async Task<PaginaDto<EncomendaDto>> ListarAsync(int usuarioId, bool admin, int pagina, int tamanho)
    {
        pagina = Math.Max(pagina, 1);
        tamanho = Math.Clamp(tamanho, 1, TamanhoMaximo);

        var consulta = _db.Encomendas.AsNoTracking().Include(e => e.Produto).AsQueryable();
        if (!admin)
            consulta = consulta.Where(e => e.UsuarioId == usuarioId);

        var total = await consulta.CountAsync();
        var itens = await consulta
            .OrderBy(e => e.DataRetirada).ThenBy(e => e.Id)
            .Skip((pagina - 1) * tamanho).Take(tamanho)
            .ToListAsync();

        return new PaginaDto<EncomendaDto>
        {
            Itens = itens.Select(e => Mapear(e, e.Produto!.Nome, e.Produto.Preco)).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)tamanho)
        };
    }

    public async Task<Resultado<EncomendaDto>> CancelarAsync(int id, int usuarioId, bool admin)
    {
        var encomenda = await _db.Encomendas.Include(e => e.Produto).FirstOrDefaultAsync(e => e.Id == id);
        if (encomenda is null)
            return Resultado<EncomendaDto>.Falha(TipoErro.NaoEncontrado, $"Encomenda {id} não encontrada.");

        if (!admin && encomenda.UsuarioId != usuarioId)
        {
            _logger.LogWarning("Usuário {UsuarioId} tentou cancelar a encomenda {EncomendaId} de outro usuário", usuarioId, id);
            return Resultado<EncomendaDto>.Falha(TipoErro.Proibido, "Você só pode cancelar as suas próprias encomendas.");
        }

        if (encomenda.Status == StatusEncomenda.Cancelada)
            return Resultado<EncomendaDto>.Falha(TipoErro.Conflito, "Esta encomenda já está cancelada.");

        encomenda.Cancelar();
        await _db.SaveChangesAsync();

        _logger.LogInformation("Encomenda {EncomendaId} cancelada pelo usuário {UsuarioId}", id, usuarioId);
        return Resultado<EncomendaDto>.Ok(Mapear(encomenda, encomenda.Produto!.Nome, encomenda.Produto.Preco));
    }

    private static EncomendaDto Mapear(Encomenda e, string produtoNome, decimal preco) => new()
    {
        Id = e.Id,
        ProdutoId = e.ProdutoId,
        ProdutoNome = produtoNome,
        UsuarioId = e.UsuarioId,
        Quantidade = e.Quantidade,
        ValorTotal = preco * e.Quantidade,
        DataRetirada = EmUtc(e.DataRetirada),
        Observacoes = e.Observacoes,
        Status = e.Status.ToString(),
        CriadoEm = EmUtc(e.CriadoEm)
    };

    private static DateTimeOffset EmUtc(DateTime d) => new(DateTime.SpecifyKind(d, DateTimeKind.Utc));
}