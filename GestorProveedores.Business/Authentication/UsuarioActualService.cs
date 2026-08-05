using GestorProveedores.Business.Ports;
using GestorProveedores.Business.Usuarios;

namespace GestorProveedores.Business.Authentication;

internal sealed class UsuarioActualService(IUsuarioReadRepository usuarioReadRepository) : IUsuarioActualService
{
    public async Task<UsuarioResponse?> ObtenerActivoAsync(int? usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId is null or <= 0)
        {
            return null;
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken);

        return usuario is null ? null : UsuarioMapper.ToResponse(usuario);
    }
}