using System.Security.Cryptography;
using Loop.Models;

namespace Loop.Services;

public sealed class UsuariosService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Usuario> _porEmail = new(StringComparer.OrdinalIgnoreCase);

    public (Usuario? Usuario, bool EmailEmUso) CadastrarMotorista(CadastroMotoristaRequest pedido)
    {
        var email = pedido.Email.Trim();
        lock (_lock)
        {
            if (_porEmail.ContainsKey(email))
                return (null, true);

            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                pedido.Senha, salt, 600_000, HashAlgorithmName.SHA256, 32);
            var usuario = new Usuario(
                Guid.NewGuid(), pedido.Nome.Trim(), email, pedido.Telefone.Trim(),
                TipoUsuario.Motorista, Convert.ToBase64String(hash),
                Convert.ToBase64String(salt), DateTimeOffset.UtcNow);
            _porEmail.Add(email, usuario);
            return (usuario, false);
        }
    }
}
