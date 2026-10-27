using Confeitaria.Api.Dtos;
using Confeitaria.Api.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Confeitaria.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthServico _auth;

    public AuthController(AuthServico auth) => _auth = auth;

    /// <summary>Cria uma conta de cliente e já devolve o token.</summary>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(RegistroDto dto)
        => this.Traduzir(await _auth.RegistrarAsync(dto),
            token => StatusCode(StatusCodes.Status201Created, token));

    /// <summary>Faz login com e-mail e senha e devolve o token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
        => this.Traduzir(await _auth.LoginAsync(dto), token => Ok(token));
}