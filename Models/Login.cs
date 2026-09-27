using System.ComponentModel.DataAnnotations;

namespace Loop.Models;

public record Login(
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, StringLength(128, MinimumLength = 8)] string Senha);

public record LoginResposta(
    string Token,
    string TipoToken,
    DateTimeOffset ExpiraEm,
    UsuarioResposta Usuario);
