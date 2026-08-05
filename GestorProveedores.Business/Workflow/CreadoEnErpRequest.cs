using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Workflow;

public sealed record CreadoEnErpRequest([property: JsonPropertyName("creado_en_erp")] bool CreadoEnErp);