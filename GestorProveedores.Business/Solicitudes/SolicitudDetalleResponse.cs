using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record SolicitudDetalleResponse(
    int Id,
    string Radicado,
    string Titulo,
    string Descripcion,
    string? Frecuencia,
    string Etapa,
    string Estado,
    [property: JsonPropertyName("requiere_aprobacion")] bool RequiereAprobacion,
    [property: JsonPropertyName("proveedor_origen")] string? ProveedorOrigen,
    [property: JsonPropertyName("solicitante_id")] int SolicitanteId,
    [property: JsonPropertyName("solicitante_nombre")] string SolicitanteNombre,
    [property: JsonPropertyName("empresa_id")] int EmpresaId,
    [property: JsonPropertyName("empresa_nombre")] string EmpresaNombre,
    [property: JsonPropertyName("aprobador_id")] int? AprobadorId,
    [property: JsonPropertyName("aprobador_nombre")] string? AprobadorNombre,
    [property: JsonPropertyName("auxiliar_id")] int? AuxiliarId,
    [property: JsonPropertyName("auxiliar_nombre")] string? AuxiliarNombre,
    [property: JsonPropertyName("analista_id")] int? AnalistaId,
    [property: JsonPropertyName("analista_nombre")] string? AnalistaNombre,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    IReadOnlyList<ProveedorCandidatoResponse> Proveedores,
    IReadOnlyList<DocumentoResponse> Documentos,
    IReadOnlyList<HistorialResponse> Historial);