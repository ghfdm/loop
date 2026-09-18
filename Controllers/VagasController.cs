using System.ComponentModel.DataAnnotations;
using Loop.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Controllers;

[ApiController]
[Route("api/vagas")]
public class VagasController(VagasService vagasService, ReservasService reservasService) : ControllerBase
{
    [HttpGet("proximas")]
    public IActionResult BuscarProximas(
        [FromQuery, Required, Range(-90d, 90d)] double? latitude,
        [FromQuery, Required, Range(-180d, 180d)] double? longitude,
        [FromQuery, Range(0.1d, 50d)] double raioKm = 2,
        [FromQuery] DateTimeOffset? inicio = null,
        [FromQuery] DateTimeOffset? fim = null)
    {
        // Required rejeita coordenadas ausentes, mas aceita zero, que é válido.
        if (!double.IsFinite(latitude!.Value) || !double.IsFinite(longitude!.Value)
            || !double.IsFinite(raioKm))
            return BadRequest(new { mensagem = "As coordenadas e o raio devem ser números finitos." });

        if (inicio.HasValue != fim.HasValue)
            return BadRequest(new { mensagem = "Informe início e fim juntos, ou omita ambos." });

        if (inicio.HasValue && !ReservasService.PeriodoValido(inicio.Value, fim!.Value, DateTimeOffset.UtcNow))
            return BadRequest(new { mensagem = "O início deve estar no futuro e o fim deve ser posterior ao início." });

        var resultados = vagasService.BuscarProximas(latitude.Value, longitude.Value, raioKm);
        if (inicio.HasValue)
            resultados = reservasService.FiltrarDisponiveis(resultados, inicio.Value, fim!.Value);
        return Ok(new
        {
            destino = new { latitude, longitude },
            raioKm,
            dadosDemonstrativos = true,
            disponibilidadeVerificada = inicio.HasValue,
            periodo = inicio.HasValue ? new { inicio, fim } : null,
            fusoHorarioFuncionamento = "UTC-03:00",
            total = resultados.Length,
            resultados
        });
    }
}
