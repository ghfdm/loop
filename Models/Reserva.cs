using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Loop.Models;

[JsonConverter(typeof(JsonStringEnumConverter<EstadoReserva>))]
public enum EstadoReserva { Confirmada, EmAndamento, Concluida }

public record Reserva(Guid Id, int VagaId, string MotoristaId,
    DateTimeOffset Inicio, DateTimeOffset Fim, DateTimeOffset CriadaEm,
    EstadoReserva Estado);

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
