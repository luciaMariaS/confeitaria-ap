using System.Security.Claims;
using System.Text;
using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Confeitaria.Api.Servicos;

public class TokenServico
{
    public const string Emissor = "confeitaria-api";
    public const string Audiencia = "confeitaria-clientes";
    private const int HorasDeValidade = 2;

    private readonly SymmetricSecurityKey _chave;

    public TokenServico(IConfiguration config)
    {
        var segredo = config["Jwt:Segredo"];
        if (string.IsNullOrWhiteSpace(segredo) || segredo.Length < 32)
            throw new InvalidOperationException("Configure Jwt__Segredo com pelo menos 32 caracteres.");

        _chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
    }

    public SymmetricSecurityKey Chave => _chave;

    public TokenDto Gerar(Usuario usuario)
    {
        var expiraEm = DateTimeOffset.UtcNow.AddHours(HorasDeValidade);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Papel)
            }),
            Issuer = Emissor,
            Audience = Audiencia,
            Expires = expiraEm.UtcDateTime,
            SigningCredentials = new SigningCredentials(_chave, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenDto
        {
            Token = token,
            ExpiraEm = expiraEm,
            Nome = usuario.Nome,
            Papel = usuario.Papel
        };
    }
}