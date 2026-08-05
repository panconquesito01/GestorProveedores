using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record SolicitudListItemResponse(
    int Id,
    string Radicado,
    string Titulo,
    string Etapa,
    string Estado,
    [property: JsonPropertyName("empresa_nombre")] string EmpresaNombre,
    [property: JsonPropertyName("solicitante_nombre")] string SolicitanteNombre,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);