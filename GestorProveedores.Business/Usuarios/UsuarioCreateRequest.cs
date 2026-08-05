using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Usuarios;

public sealed record UsuarioCreateRequest(
    string Nombre,
    string Email,
    string Username,
    string Password,
    string Rol,
    [property: JsonPropertyName("empresa_id")] int? EmpresaId);