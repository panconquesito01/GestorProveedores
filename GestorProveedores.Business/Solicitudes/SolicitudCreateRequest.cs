using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record SolicitudCreateRequest(
    string Titulo,
    string Descripcion,
    string? Frecuencia,
    [property: JsonPropertyName("aprobador_id")] int? AprobadorId);