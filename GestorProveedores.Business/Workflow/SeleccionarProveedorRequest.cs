using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Workflow;

public sealed record SeleccionarProveedorRequest(
    [property: JsonPropertyName("proveedor_id")] int ProveedorId,
    string? Comentario);