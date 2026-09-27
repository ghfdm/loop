using System.ComponentModel.DataAnnotations;

namespace Loop.Models;

public record CadastroMotoristaRequest(
    [property: Required, StringLength(100, MinimumLength = 2)] string Nome,
    [property: Required, EmailAddress, StringLength(254)] string Email,
    [property: Required, Phone, StringLength(20, MinimumLength = 8)] string Telefone,
    [property: Required, StringLength(128, MinimumLength = 8)] string Senha);
