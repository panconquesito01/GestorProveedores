using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record HistorialResponse(
    int Id,
    string Etapa,
    string Accion,
    [property: JsonPropertyName("actor_id")] int? ActorId,
    [property: JsonPropertyName("actor_nombre")] string? ActorNombre,
    string? Comentario,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);