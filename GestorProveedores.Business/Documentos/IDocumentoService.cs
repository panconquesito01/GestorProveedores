namespace GestorProveedores.Business.Documentos;

public interface IDocumentoService
{
    Task<DocumentoDownloadResponse> DescargarAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default);
}