using Confeitaria.Api.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Confeitaria.Api.Controllers;

public static class ResultadoExtensoes
{
    public static IActionResult Traduzir<T>(
        this ControllerBase controller,
        Resultado<T> resultado,
        Func<T, IActionResult> sucesso)
    {
        if (resultado.Sucesso)
            return sucesso(resultado.Valor!);

        var (status, titulo) = resultado.Erro switch
        {
            TipoErro.Validacao => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            TipoErro.NaoAutenticado => (StatusCodes.Status401Unauthorized, "Não autenticado"),
            TipoErro.Proibido => (StatusCodes.Status403Forbidden, "Acesso negado"),
            TipoErro.NaoEncontrado => (StatusCodes.Status404NotFound, "Não encontrado"),
            TipoErro.Conflito => (StatusCodes.Status409Conflict, "Conflito"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        return controller.Problem(detail: resultado.Mensagem, statusCode: status, title: titulo);
    }
}