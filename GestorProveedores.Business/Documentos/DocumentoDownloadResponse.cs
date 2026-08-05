namespace GestorProveedores.Business.Documentos;

public sealed record DocumentoDownloadResponse(
    string NombreArchivo,
    string MimeType,
    byte[] Contenido);