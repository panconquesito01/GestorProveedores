using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(Usuario usuario);
}

public sealed record JwtTokenResult(string AccessToken, int ExpiresIn);