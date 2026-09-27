namespace Loop.Models;

public enum TipoUsuario
{
    Motorista,
    Proprietario
}

public record Usuario(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    TipoUsuario Tipo,
    string HashSenha,
    string SaltSenha,
    DateTimeOffset CriadoEm);

// DTO retornado pela API: nunca inclui hash ou salt da senha.
public record UsuarioResposta(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    TipoUsuario Tipo,
    DateTimeOffset CriadoEm)
{
    public static UsuarioResposta De(Usuario usuario) => new(
        usuario.Id, usuario.Nome, usuario.Email, usuario.Telefone,
        usuario.Tipo, usuario.CriadoEm);
}
