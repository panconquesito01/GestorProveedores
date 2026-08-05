using GestorProveedores.Business.Ports;
using GestorProveedores.Business.Usuarios;

namespace GestorProveedores.Business.Authentication;

internal sealed class AuthService(
    IUsuarioReadRepository usuarioReadRepository,
    IPasswordHashVerifier passwordHashVerifier,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Identificador) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var usuario = await usuarioReadRepository.GetActiveByIdentifierAsync(request.Identificador.Trim(), cancellationToken);

        if (usuario is null || !passwordHashVerifier.Verify(usuario, request.Password))
        {
            return null;
        }

        var token = jwtTokenService.CreateToken(usuario);

        return new LoginResponse(UsuarioMapper.ToResponse(usuario), token.AccessToken, "Bearer", token.ExpiresIn);
    }
}