using Loop.Models;

namespace Loop.Services;

public enum FalhaReserva { Nenhuma, PeriodoInvalido, VagaInexistente, Conflito }
public record ResultadoCriacaoReserva(Reserva? Reserva, FalhaReserva Falha);

public sealed class ReservasService(VagasService vagasService)
{
    private readonly Dictionary<Guid, Reserva> _reservas = new();
    private readonly object _controle = new();

    public ResultadoCriacaoReserva Criar(int vagaId, string motoristaId,
        DateTimeOffset inicio, DateTimeOffset fim)
    {
        // O mesmo bloqueio protege a verificação e a criação. Duas chamadas
        // simultâneas não conseguem reservar a mesma vaga para o mesmo período.
        lock (_controle)
        {
            var agora = DateTimeOffset.UtcNow;
            if (inicio <= agora || fim <= inicio)
                return new(null, FalhaReserva.PeriodoInvalido);

            if (vagasService.BuscarPorId(vagaId) is null)
                return new(null, FalhaReserva.VagaInexistente);

            // Intervalo [início, fim): permite uma reserva começar quando outra termina.
            var conflito = _reservas.Values.Any(reserva => reserva.VagaId == vagaId
                && inicio < reserva.Fim && fim > reserva.Inicio);
            if (conflito)
                return new(null, FalhaReserva.Conflito);

            var reserva = new Reserva(Guid.NewGuid(), vagaId, motoristaId.Trim(),
                inicio.ToUniversalTime(), fim.ToUniversalTime(), agora, "Confirmada");
            _reservas.Add(reserva.Id, reserva);
            return new(reserva, FalhaReserva.Nenhuma);
        }
    }

    public Reserva? BuscarPorId(Guid id)
    {
        lock (_controle)
            return _reservas.GetValueOrDefault(id);
    }
}
