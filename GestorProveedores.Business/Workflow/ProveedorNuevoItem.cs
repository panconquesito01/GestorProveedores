using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Workflow;

public sealed record ProveedorNuevoItem(
    string Nombre,
    string? Nit,
    [property: JsonPropertyName("correo_contacto")] string? CorreoContacto,
    [property: JsonPropertyName("telefono_contacto")] string? TelefonoContacto,
    bool Validado = false);