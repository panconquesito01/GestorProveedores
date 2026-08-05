using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Ports;

public interface IDocumentoRepository
{
    Task<Documento?> GetByIdWithSolicitudAsync(int id, CancellationToken cancellationToken = default);
}