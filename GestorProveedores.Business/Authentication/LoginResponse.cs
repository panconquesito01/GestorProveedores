using GestorProveedores.Business.Usuarios;
using System.Text.Json.Serialization;

namespace GestorProveedores.Business.Authentication;

public sealed record LoginResponse(
	UsuarioResponse Usuario,
	[property: JsonPropertyName("access_token")] string AccessToken,
	[property: JsonPropertyName("token_type")] string TokenType,
	[property: JsonPropertyName("expires_in")] int ExpiresIn);