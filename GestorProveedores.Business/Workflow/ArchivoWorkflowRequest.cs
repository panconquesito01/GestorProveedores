namespace GestorProveedores.Business.Workflow;

public sealed record ArchivoWorkflowRequest(
    string NombreArchivo,
    string MimeType,
    byte[] Contenido);