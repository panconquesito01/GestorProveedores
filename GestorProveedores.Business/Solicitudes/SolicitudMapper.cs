using GestorProveedores.Business.Common;
using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Solicitudes;

internal static class SolicitudMapper
{
    public static SolicitudListItemResponse ToListItem(Solicitud solicitud) => new(
        solicitud.Id,
        solicitud.Radicado,
        solicitud.Titulo,
        EnumText.ToApiValue(solicitud.Etapa),
        EnumText.ToApiValue(solicitud.Estado),
        solicitud.Empresa.Nombre,
        solicitud.Solicitante.Nombre,
        solicitud.CreatedAt,
        solicitud.UpdatedAt);

    public static SolicitudDetalleResponse ToDetalle(Solicitud solicitud) => new(
        solicitud.Id,
        solicitud.Radicado,
        solicitud.Titulo,
        solicitud.Descripcion,
        solicitud.Frecuencia,
        EnumText.ToApiValue(solicitud.Etapa),
        EnumText.ToApiValue(solicitud.Estado),
        solicitud.RequiereAprobacion,
        solicitud.ProveedorOrigen.HasValue ? EnumText.ToApiValue(solicitud.ProveedorOrigen.Value) : null,
        solicitud.SolicitanteId,
        solicitud.Solicitante.Nombre,
        solicitud.EmpresaId,
        solicitud.Empresa.Nombre,
        solicitud.AprobadorId,
        solicitud.Aprobador?.Nombre,
        solicitud.AuxiliarId,
        solicitud.Auxiliar?.Nombre,
        solicitud.AnalistaId,
        solicitud.Analista?.Nombre,
        solicitud.CreatedAt,
        solicitud.UpdatedAt,
        solicitud.Proveedores.OrderBy(proveedor => proveedor.Orden).Select(ToProveedor).ToList(),
        solicitud.Documentos.Select(ToDocumento).ToList(),
        solicitud.Historial.OrderBy(historial => historial.CreatedAt).Select(ToHistorial).ToList());

    private static ProveedorCandidatoResponse ToProveedor(ProveedorCandidato proveedor) => new(
        proveedor.Id,
        proveedor.Orden,
        EnumText.ToApiValue(proveedor.Origen),
        proveedor.Nombre,
        proveedor.Nit,
        proveedor.IdentificadorErp,
        proveedor.CorreoContacto,
        proveedor.TelefonoContacto,
        proveedor.Validado,
        proveedor.CreadoEnErp,
        proveedor.Seleccionado);

    private static DocumentoResponse ToDocumento(Documento documento) => new(
        documento.Id,
        EnumText.ToApiValue(documento.Tipo),
        documento.NombreArchivo,
        documento.MimeType,
        documento.ProveedorCandidatoId,
        documento.SubidoPor,
        documento.CreatedAt);

    private static HistorialResponse ToHistorial(SolicitudHistorial historial) => new(
        historial.Id,
        EnumText.ToApiValue(historial.Etapa),
        historial.Accion,
        historial.ActorId,
        historial.Actor?.Nombre ?? "Sistema",
        historial.Comentario,
        historial.CreatedAt);
}