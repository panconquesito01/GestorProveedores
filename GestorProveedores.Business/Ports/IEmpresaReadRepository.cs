using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Ports;

public interface IEmpresaReadRepository
{
    Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default);
}