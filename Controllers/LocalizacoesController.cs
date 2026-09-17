using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Loop.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Controllers;

[ApiController]
[Route("api/localizacoes")]
public class LocalizacoesController(
    NominatimService nominatim,
    ILogger<LocalizacoesController> logger) : ControllerBase
{
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery, Required, StringLength(200, MinimumLength = 3)] string endereco,
        CancellationToken cancellationToken)
    {
        if (endereco.Trim().Length < 3)
            return BadRequest(new { mensagem = "Informe um endereço com pelo menos 3 caracteres." });

        try
        {
            var locais = await nominatim.BuscarAsync(endereco, cancellationToken);
            return Ok(new
            {
                atribuicao = "© OpenStreetMap contributors",
                fonte = "https://www.openstreetmap.org/copyright",
                resultados = locais
            });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Problem(statusCode: 504, title: "O serviço de localização demorou para responder.");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or FormatException or OverflowException)
        {
            logger.LogWarning(ex, "Falha ao consultar o Nominatim.");
            return Problem(statusCode: 502, title: "Não foi possível consultar o serviço de localização. Tente novamente mais tarde.");
        }
    }
}
