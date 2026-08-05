using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;

namespace GestorProveedores.Business.Ports;

public interface IUsuarioReadRepository
{
    Task<Usuario?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Usuario?> GetActiveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListActiveApproversByEmpresaAsync(int empresaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListActiveByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default);
}