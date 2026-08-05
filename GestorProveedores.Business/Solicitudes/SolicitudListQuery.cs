using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record SolicitudListQuery(
    string Vista,
    [property: JsonPropertyName("solicitante_nombre")] string? SolicitanteNombre,
    [property: JsonPropertyName("empresa_id")] int? EmpresaId,
    [property: JsonPropertyName("fecha_desde")] DateOnly? FechaDesde,
    [property: JsonPropertyName("fecha_hasta")] DateOnly? FechaHasta);