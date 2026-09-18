using System.ComponentModel.DataAnnotations;
using Loop.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Controllers;

[ApiController]
[Route("api/vagas")]
public class VagasController(VagasService vagasService) : ControllerBase
{
    [HttpGet("proximas")]
    public IActionResult BuscarProximas(
        [FromQuery, Required, Range(-90d, 90d)] double? latitude,
        [FromQuery, Required, Range(-180d, 180d)] double? longitude,
        [FromQuery, Range(0.1d, 50d)] double raioKm = 2)
    {
        // Required rejeita coordenadas ausentes, mas aceita zero, que é válido.
        if (!double.IsFinite(latitude!.Value) || !double.IsFinite(longitude!.Value)
            || !double.IsFinite(raioKm))
            return BadRequest(new { mensagem = "As coordenadas e o raio devem ser números finitos." });

        var resultados = vagasService.BuscarProximas(latitude.Value, longitude.Value, raioKm);
        return Ok(new
        {
            destino = new { latitude, longitude },
            raioKm,
            dadosDemonstrativos = true,
            total = resultados.Length,
            resultados
        });
    }
}
