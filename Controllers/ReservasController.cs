using Loop.Models;
using Loop.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Controllers;

[ApiController]
[Route("api/reservas")]
public class ReservasController(ReservasService reservasService) : ControllerBase
{
    [HttpPost]
    public IActionResult Criar([FromBody] CriarReservaRequest request)
    {
        var resultado = reservasService.Criar(request.VagaId!.Value,
            request.MotoristaId, request.Inicio!.Value, request.Fim!.Value);

        return resultado.Falha switch
        {
            FalhaReserva.PeriodoInvalido => BadRequest(new
            {
                mensagem = "O início deve estar no futuro e o fim deve ser posterior ao início."
            }),
            FalhaReserva.VagaInexistente => NotFound(new { mensagem = "Vaga não encontrada." }),
            FalhaReserva.ForaDoHorario => Conflict(new
            {
                mensagem = "O período solicitado está fora do horário de funcionamento da vaga (UTC-03:00)."
            }),
            FalhaReserva.Conflito => Conflict(new
            {
                mensagem = "A vaga já está reservada em parte ou em todo esse período."
            }),
            _ => CreatedAtAction(nameof(BuscarPorId), new { id = resultado.Reserva!.Id }, resultado.Reserva)
        };
    }

    [HttpGet("{id:guid}")]
    public IActionResult BuscarPorId(Guid id)
    {
        var reserva = reservasService.BuscarPorId(id);
        return reserva is null
            ? NotFound(new { mensagem = "Reserva não encontrada." })
            : Ok(reserva);
    }

    [HttpPost("{id:guid}/iniciar")]
    public IActionResult Iniciar(Guid id) => ResponderTransicao(reservasService.Iniciar(id));

    [HttpPost("{id:guid}/concluir")]
    public IActionResult Concluir(Guid id) => ResponderTransicao(reservasService.Concluir(id));

    [HttpPost("{id:guid}/cancelar")]
    public IActionResult Cancelar(Guid id) => ResponderTransicao(reservasService.Cancelar(id));

    private IActionResult ResponderTransicao(ResultadoTransicao resultado)
    {
        return resultado.Falha switch
        {
            FalhaTransicao.ReservaInexistente => NotFound(new { mensagem = "Reserva não encontrada." }),
            FalhaTransicao.EstadoInvalido => Conflict(new
            {
                mensagem = "O estado atual da reserva não permite essa operação."
            }),
            FalhaTransicao.HorarioInvalido => Conflict(new
            {
                mensagem = "Para iniciar, aguarde o início e faça o pedido antes do fim. Para concluir, aguarde o fim do período reservado. Para cancelar, faça o pedido antes do início."
            }),
            _ => Ok(resultado.Reserva)
        };
    }
}
