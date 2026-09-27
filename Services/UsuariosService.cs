using System.Security.Cryptography;
using Loop.Models;

namespace Loop.Services;

public sealed class UsuariosService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Usuario> _porEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (Guid UsuarioId, DateTimeOffset ExpiraEm)> _sessoes = new();

    private static readonly TimeSpan DuracaoSessao = TimeSpan.FromHours(8);

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

    public LoginResposta? Entrar(Login pedido)
    {
        lock (_lock)
        {
            foreach (var tokenExpirado in _sessoes
                .Where(s => s.Value.ExpiraEm <= DateTimeOffset.UtcNow)
                .Select(s => s.Key).ToArray())
                _sessoes.Remove(tokenExpirado);

            if (!_porEmail.TryGetValue(pedido.Email.Trim(), out var usuario))
                return null;

            var hashEsperado = Convert.FromBase64String(usuario.HashSenha);
            var salt = Convert.FromBase64String(usuario.SaltSenha);
            var hashRecebido = Rfc2898DeriveBytes.Pbkdf2(
                pedido.Senha, salt, 600_000, HashAlgorithmName.SHA256, hashEsperado.Length);
            if (!CryptographicOperations.FixedTimeEquals(hashEsperado, hashRecebido))
                return null;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var expiraEm = DateTimeOffset.UtcNow.Add(DuracaoSessao);
            _sessoes[token] = (usuario.Id, expiraEm);
            return new LoginResposta(token, "Bearer", expiraEm, UsuarioResposta.De(usuario));
        }
    }

    public (Usuario? Usuario, bool TokenInvalido, bool EmailEmUso) AtualizarPerfil(
        string token, AtualizarPerfil pedido)
    {
        lock (_lock)
        {
            if (!_sessoes.TryGetValue(token, out var sessao))
                return (null, true, false);

            if (sessao.ExpiraEm <= DateTimeOffset.UtcNow)
            {
                _sessoes.Remove(token);
                return (null, true, false);
            }

            var atual = _porEmail.Values.FirstOrDefault(u => u.Id == sessao.UsuarioId);
            if (atual is null)
                return (null, true, false);

            var novoEmail = pedido.Email.Trim();
            if (_porEmail.TryGetValue(novoEmail, out var donoEmail) && donoEmail.Id != atual.Id)
                return (null, false, true);

            var atualizado = atual with
            {
                Nome = pedido.Nome.Trim(),
                Email = novoEmail,
                Telefone = pedido.Telefone.Trim()
            };
            _porEmail.Remove(atual.Email);
            _porEmail[novoEmail] = atualizado;
            return (atualizado, false, false);
        }
    }
}
