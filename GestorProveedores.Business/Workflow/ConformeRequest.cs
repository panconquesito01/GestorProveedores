using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Workflow;

public sealed record ConformeRequest([property: JsonPropertyName("confirmacion_erp")] bool ConfirmacionErp);