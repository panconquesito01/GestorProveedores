using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record DocumentoResponse(
    int Id,
    string Tipo,
    [property: JsonPropertyName("nombre_archivo")] string NombreArchivo,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("proveedor_candidato_id")] int? ProveedorCandidatoId,
    [property: JsonPropertyName("subido_por")] int SubidoPor,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);