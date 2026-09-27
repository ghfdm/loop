using Loop.Models;
using Loop.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(UsuariosService usuariosService) : ControllerBase
{
    [HttpPost("motoristas")]
    public ActionResult<UsuarioResposta> CadastrarMotorista(CadastroMotorista pedido)
    {
        var (usuario, emailEmUso) = usuariosService.CadastrarMotorista(pedido);
        if (emailEmUso)
            return Conflict(new { mensagem = "Este e-mail já está cadastrado." });

        return Created($"/api/usuarios/{usuario!.Id}", UsuarioResposta.De(usuario));
    }

    [HttpPost("proprietarios")]
    public ActionResult<UsuarioResposta> CadastrarProprietario(CadastroProprietario pedido)
    {
        var (usuario, emailEmUso) = usuariosService.CadastrarProprietario(pedido);
        if (emailEmUso)
            return Conflict(new { mensagem = "Este e-mail já está cadastrado." });

        return Created($"/api/usuarios/{usuario!.Id}", UsuarioResposta.De(usuario));
    }
}
