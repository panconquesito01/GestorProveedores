using GestorProveedores.Business.Solicitudes;

namespace GestorProveedores.Business.Workflow;

public interface IWorkflowService
{
    Task<SolicitudDetalleResponse> DevolverPaso1Async(
        int id,
        ComentarioRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> AvanzarPaso1Async(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> RegistrarProveedorErpAsync(
        int id,
        ProveedorErpRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> AvanzarProveedorErpAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> GuardarProveedoresNuevosAsync(
        int id,
        ProveedoresNuevosRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> MarcarProveedorCreadoEnErpAsync(
        int id,
        int proveedorId,
        CreadoEnErpRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> AvanzarProveedoresNuevosAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> SeleccionarProveedorAsync(
        int id,
        SeleccionarProveedorRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> SubirDocumentoProveedorAsync(
        int id,
        int proveedorId,
        string tipo,
        ArchivoWorkflowRequest archivo,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> CargarOrdenCompraAsync(
        int id,
        ArchivoWorkflowRequest archivo,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> RevisarOrdenCompraSolicitanteAsync(
        int id,
        DecisionRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> RevisarOrdenCompraAprobadorAsync(
        int id,
        DecisionRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> CargarFacturaAsync(
        int id,
        ArchivoWorkflowRequest archivo,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> RevisarFacturaSolicitanteAsync(
        int id,
        DecisionRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> EnviarContabilidadAsync(
        int id,
        IReadOnlyList<ArchivoWorkflowRequest> soportes,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> MarcarConformeContabilidadAsync(
        int id,
        ConformeRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> ObjetarContabilidadAsync(
        int id,
        ObjetarContableRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleResponse> ReenviarFacturaObjetadaAsync(
        int id,
        ArchivoWorkflowRequest archivo,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default);
}