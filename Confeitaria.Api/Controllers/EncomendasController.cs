using System.Security.Claims;
using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Confeitaria.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Confeitaria.Api.Controllers;

[ApiController]
[Route("api/encomendas")]
[Authorize]
public class EncomendasController : ControllerBase
{
    private readonly EncomendaServico _encomendas;

    public EncomendasController(EncomendaServico encomendas) => _encomendas = encomendas;

    private bool TentarObterUsuarioId(out int id)
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);

    /// <summary>Cria uma encomenda para o usuário logado. Retorna 409 se a data estiver lotada.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EncomendaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(EncomendaEntradaDto dto)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        return this.Traduzir(await _encomendas.CriarAsync(usuarioId, dto),
            encomenda => StatusCode(StatusCodes.Status201Created, encomenda));
    }

    /// <summary>Lista as encomendas do usuário logado (o admin vê todas).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<EncomendaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        var admin = User.IsInRole(Usuario.PapelAdmin);
        return Ok(await _encomendas.ListarAsync(usuarioId, admin, pagina, tamanho));
    }

    /// <summary>Cancela uma encomenda. Só o dono ou o admin.</summary>
    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(EncomendaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        var admin = User.IsInRole(Usuario.PapelAdmin);
        return this.Traduzir(await _encomendas.CancelarAsync(id, usuarioId, admin), encomenda => Ok(encomenda));
    }
}