using System.ComponentModel.DataAnnotations;

namespace Loop.Models;

public record Reserva(Guid Id, int VagaId, string MotoristaId,
    DateTimeOffset Inicio, DateTimeOffset Fim, DateTimeOffset CriadaEm,
    string Estado);

// Dados enviados no corpo JSON da requisição de criação.
public sealed class CriarReservaRequest
{
    [Required, Range(1, int.MaxValue)]
    public int? VagaId { get; init; }

    [Required, StringLength(100)]
    public string MotoristaId { get; init; } = "";

    [Required]
    public DateTimeOffset? Inicio { get; init; }

    [Required]
    public DateTimeOffset? Fim { get; init; }
}
