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
}
