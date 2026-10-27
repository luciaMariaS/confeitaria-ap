using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Confeitaria.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Confeitaria.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoServico _produtos;

    public ProdutosController(ProdutoServico produtos) => _produtos = produtos;

    /// <summary>Lista os produtos com busca e paginação. Público.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busca, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var admin = User.IsInRole(Usuario.PapelAdmin);
        return Ok(await _produtos.ListarAsync(busca, pagina, tamanho, incluirInativos: admin));
    }

    /// <summary>Mostra um produto pelo id. Público.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id)
        => this.Traduzir(await _produtos.ObterAsync(id), produto => Ok(produto));

    /// <summary>Cria um produto. Só admin.</summary>
    [HttpPost]
    [Authorize(Roles = Usuario.PapelAdmin)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar(ProdutoEntradaDto dto)
        => this.Traduzir(await _produtos.CriarAsync(dto),
            produto => CreatedAtAction(nameof(Obter), new { id = produto.Id }, produto));

    /// <summary>Atualiza um produto. Só admin.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Usuario.PapelAdmin)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, ProdutoEntradaDto dto)
        => this.Traduzir(await _produtos.AtualizarAsync(id, dto), produto => Ok(produto));

    /// <summary>Exclui um produto sem encomendas. Só admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Usuario.PapelAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
        => this.Traduzir(await _produtos.ExcluirAsync(id), _ => NoContent());
}