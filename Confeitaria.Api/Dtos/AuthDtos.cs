using System.ComponentModel.DataAnnotations;

namespace Confeitaria.Api.Dtos;

public class RegistroDto
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Senha { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiraEm { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Papel { get; set; } = string.Empty;
}