using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Solicitudes;

public sealed record ProveedorCandidatoResponse(
    int Id,
    short Orden,
    string Origen,
    string Nombre,
    string? Nit,
    [property: JsonPropertyName("identificador_erp")] string? IdentificadorErp,
    [property: JsonPropertyName("correo_contacto")] string? CorreoContacto,
    [property: JsonPropertyName("telefono_contacto")] string? TelefonoContacto,
    bool Validado,
    [property: JsonPropertyName("creado_en_erp")] bool CreadoEnErp,
    bool Seleccionado);