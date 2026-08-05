using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Workflow;

public sealed record ProveedorErpRequest(
    string Nombre,
    string? Nit,
    [property: JsonPropertyName("identificador_erp")] string? IdentificadorErp,
    [property: JsonPropertyName("correo_contacto")] string? CorreoContacto,
    [property: JsonPropertyName("telefono_contacto")] string? TelefonoContacto);