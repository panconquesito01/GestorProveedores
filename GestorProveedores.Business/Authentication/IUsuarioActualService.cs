using GestorProveedores.Business.Usuarios;

namespace GestorProveedores.Business.Authentication;

public interface IUsuarioActualService
{
    Task<UsuarioResponse?> ObtenerActivoAsync(int? usuarioId, CancellationToken cancellationToken = default);
}