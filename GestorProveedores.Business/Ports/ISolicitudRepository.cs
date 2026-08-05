using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Ports;

public interface ISolicitudRepository
{
    Task<IReadOnlyList<Solicitud>> ListAsync(SolicitudListCriteria criteria, CancellationToken cancellationToken = default);

    Task<Solicitud?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Solicitud?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    void Add(Solicitud solicitud);

    void AddProveedor(ProveedorCandidato proveedor);

    void RemoveProveedor(ProveedorCandidato proveedor);

    void AddDocumento(Documento documento);

    void AddHistorial(SolicitudHistorial historial);
}