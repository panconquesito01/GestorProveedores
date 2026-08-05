using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Business.Documentos;

internal sealed class DocumentoService(
    IUsuarioReadRepository usuarioReadRepository,
    IDocumentoRepository documentoRepository) : IDocumentoService
{
    public async Task<DocumentoDownloadResponse> DescargarAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerUsuarioActualAsync(usuarioId, cancellationToken);
        var documento = await documentoRepository.GetByIdWithSolicitudAsync(id, cancellationToken)
            ?? throw new NotFoundException("documentos.no_encontrado", "Documento no encontrado.");

        if (!TieneAcceso(usuario, documento.Solicitud))
        {
            throw new ForbiddenException("documentos.sin_permiso", "No tienes acceso a este documento.");
        }

        return new DocumentoDownloadResponse(documento.NombreArchivo, documento.MimeType, documento.Contenido);
    }

    private async Task<Usuario> ObtenerUsuarioActualAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        return await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");
    }

    private static bool TieneAcceso(Usuario usuario, Solicitud solicitud) => usuario.Rol switch
    {
        RolUsuario.Solicitante => solicitud.SolicitanteId == usuario.Id,
        RolUsuario.Auxiliar => solicitud.AuxiliarId == usuario.Id,
        RolUsuario.Analista => solicitud.AnalistaId == usuario.Id,
        RolUsuario.Aprobador => solicitud.AprobadorId == usuario.Id,
        RolUsuario.Contable => true,
        _ => false
    };
}