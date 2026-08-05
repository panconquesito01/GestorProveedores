using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Usuarios;

public sealed record UsuarioResponse(
    int Id,
    string Nombre,
    string Email,
    string Username,
    string Rol,
    [property: JsonPropertyName("empresa_id")] int? EmpresaId,
    [property: JsonPropertyName("empresa_nombre")] string? EmpresaNombre);