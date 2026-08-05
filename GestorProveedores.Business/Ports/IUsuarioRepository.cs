using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Ports;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default);

    Task<Usuario?> GetByEmailOrUsernameAsync(string email, string username, CancellationToken cancellationToken = default);

    void Add(Usuario usuario);
}