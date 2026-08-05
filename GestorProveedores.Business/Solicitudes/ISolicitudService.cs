namespace GestorProveedores.Business.Solicitudes;

public interface ISolicitudService
{
    Task<IReadOnlyList<SolicitudListItemResponse>> ListarAsync(
        SolicitudListQuery query,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> CrearAsync(
        SolicitudCreateRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> EditarYReenviarAsync(
        int id,
        SolicitudUpdateRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> ObtenerDetalleAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default);
}