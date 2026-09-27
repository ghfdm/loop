using System.Security.Cryptography;
using Loop.Models;

namespace Loop.Services;

public sealed class UsuariosService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Usuario> _porEmail = new(StringComparer.OrdinalIgnoreCase);

    public (Usuario? Usuario, bool EmailEmUso) CadastrarMotorista(CadastroMotorista pedido)
        => Cadastrar(pedido.Nome, pedido.Email, pedido.Telefone, pedido.Senha, TipoUsuario.Motorista);

    public (Usuario? Usuario, bool EmailEmUso) CadastrarProprietario(CadastroProprietario pedido)
        => Cadastrar(pedido.Nome, pedido.Email, pedido.Telefone, pedido.Senha, TipoUsuario.Proprietario);

    private (Usuario? Usuario, bool EmailEmUso) Cadastrar(
        string nome, string emailInformado, string telefone, string senha, TipoUsuario tipo)
    {
        var email = emailInformado.Trim();
        lock (_lock)
        {
            if (_porEmail.ContainsKey(email))
                return (null, true);

            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                senha, salt, 600_000, HashAlgorithmName.SHA256, 32);
            var usuario = new Usuario(
                Guid.NewGuid(), nome.Trim(), email, telefone.Trim(),
                tipo, Convert.ToBase64String(hash),
                Convert.ToBase64String(salt), DateTimeOffset.UtcNow);
            _porEmail.Add(email, usuario);
            return (usuario, false);
        }
    }
}
