using Confeitaria.Api.Dados;
using Confeitaria.Api.Dtos;
using Confeitaria.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Confeitaria.Api.Servicos;

public class AuthServico
{
    private readonly AppDbContext _db;
    private readonly TokenServico _tokens;
    private readonly ILogger<AuthServico> _logger;

    public AuthServico(AppDbContext db, TokenServico tokens, ILogger<AuthServico> logger)
    {
        _db = db;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<Resultado<TokenDto>> RegistrarAsync(RegistroDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
        {
            _logger.LogWarning("Registro recusado: e-mail {Email} já cadastrado", email);
            return Resultado<TokenDto>.Falha(TipoErro.Conflito, "Já existe uma conta com esse e-mail.");
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var usuario = new Usuario(dto.Nome, email, hash, Usuario.PapelCliente);

        _db.Usuarios.Add(usuario);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // duas requisições ao mesmo tempo: o índice único do banco é a última barreira
            _logger.LogWarning("Registro recusado pelo índice único: e-mail {Email}", email);
            return Resultado<TokenDto>.Falha(TipoErro.Conflito, "Já existe uma conta com esse e-mail.");
        }

        _logger.LogInformation("Usuário {UsuarioId} registrado como {Papel}", usuario.Id, usuario.Papel);
        return Resultado<TokenDto>.Ok(_tokens.Gerar(usuario));
    }

    public async Task<Resultado<TokenDto>> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Login recusado para o e-mail {Email}", email);
            return Resultado<TokenDto>.Falha(TipoErro.NaoAutenticado, "E-mail ou senha inválidos.");
        }

        _logger.LogInformation("Login do usuário {UsuarioId}", usuario.Id);
        return Resultado<TokenDto>.Ok(_tokens.Gerar(usuario));
    }
}