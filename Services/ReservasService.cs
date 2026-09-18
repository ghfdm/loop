using Loop.Models;

namespace Loop.Services;

public enum FalhaReserva { Nenhuma, PeriodoInvalido, VagaInexistente, ForaDoHorario, Conflito }
public record ResultadoCriacaoReserva(Reserva? Reserva, FalhaReserva Falha);
public enum FalhaTransicao { Nenhuma, ReservaInexistente, EstadoInvalido, HorarioInvalido }
public record ResultadoTransicao(Reserva? Reserva, FalhaTransicao Falha);

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
            if (!PeriodoValido(inicio, fim, agora))
                return new(null, FalhaReserva.PeriodoInvalido);

            var vaga = vagasService.BuscarPorId(vagaId);
            if (vaga is null)
                return new(null, FalhaReserva.VagaInexistente);

            if (!vagasService.EstaNoHorarioDeFuncionamento(vaga, inicio, fim))
                return new(null, FalhaReserva.ForaDoHorario);

            // Intervalo [início, fim): permite uma reserva começar quando outra termina.
            var conflito = TemConflito(vagaId, inicio, fim);
            if (conflito)
                return new(null, FalhaReserva.Conflito);

            var reserva = new Reserva(Guid.NewGuid(), vagaId, motoristaId.Trim(),
                inicio.ToUniversalTime(), fim.ToUniversalTime(), agora, EstadoReserva.Confirmada);
            _reservas.Add(reserva.Id, reserva);
            return new(reserva, FalhaReserva.Nenhuma);
        }
    }

    public Reserva? BuscarPorId(Guid id)
    {
        lock (_controle)
            return _reservas.GetValueOrDefault(id);
    }

    public static bool PeriodoValido(DateTimeOffset inicio, DateTimeOffset fim, DateTimeOffset agora)
        => inicio > agora && fim > inicio;

    public VagaProxima[] FiltrarDisponiveis(VagaProxima[] vagas,
        DateTimeOffset inicio, DateTimeOffset fim)
    {
        // Uma única leitura protegida oferece resultados consistentes nesta consulta.
        lock (_controle)
            return vagas.Where(resultado =>
                vagasService.EstaNoHorarioDeFuncionamento(resultado.Vaga, inicio, fim)
                && !TemConflito(resultado.Vaga.Id, inicio, fim)).ToArray();
    }

    // Chamado somente dentro do lock. Concluídas e canceladas não bloqueiam a agenda.
    private bool TemConflito(int vagaId, DateTimeOffset inicio, DateTimeOffset fim)
        => _reservas.Values.Any(reserva => reserva.VagaId == vagaId
            && (reserva.Estado is EstadoReserva.Confirmada or EstadoReserva.EmAndamento)
            && inicio < reserva.Fim && fim > reserva.Inicio);

    public ResultadoTransicao Iniciar(Guid id) => AlterarEstado(id, EstadoReserva.EmAndamento);

    public ResultadoTransicao Concluir(Guid id) => AlterarEstado(id, EstadoReserva.Concluida);

    public ResultadoTransicao Cancelar(Guid id) => AlterarEstado(id, EstadoReserva.Cancelada);

    private ResultadoTransicao AlterarEstado(Guid id, EstadoReserva destino)
    {
        lock (_controle)
        {
            if (!_reservas.TryGetValue(id, out var reserva))
                return new(null, FalhaTransicao.ReservaInexistente);

            // Só existem os caminhos definidos aqui. Não aceitamos estado arbitrário do cliente.
            var transicaoPermitida = (reserva.Estado, destino) switch
            {
                (EstadoReserva.Confirmada, EstadoReserva.EmAndamento) => true,
                (EstadoReserva.EmAndamento, EstadoReserva.Concluida) => true,
                // Permite encerrar uma reserva vencida mesmo sem registro de início.
                (EstadoReserva.Confirmada, EstadoReserva.Concluida) => true,
                (EstadoReserva.Confirmada, EstadoReserva.Cancelada) => true,
                _ => false
            };
            if (!transicaoPermitida)
                return new(null, FalhaTransicao.EstadoInvalido);

            var agora = DateTimeOffset.UtcNow;
            var horarioPermitido = destino switch
            {
                EstadoReserva.EmAndamento => agora >= reserva.Inicio && agora < reserva.Fim,
                EstadoReserva.Concluida => agora >= reserva.Fim,
                EstadoReserva.Cancelada => agora < reserva.Inicio,
                _ => false
            };
            if (!horarioPermitido)
                return new(null, FalhaTransicao.HorarioInvalido);

            // record + with cria uma cópia; substituímos o registro dentro do bloqueio.
            var atualizada = reserva with
            {
                Estado = destino,
                CanceladaEm = destino == EstadoReserva.Cancelada ? agora : reserva.CanceladaEm
            };
            _reservas[id] = atualizada;
            return new(atualizada, FalhaTransicao.Nenhuma);
        }
    }
}
