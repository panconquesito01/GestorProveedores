using GestorProveedores.Business.Common;
using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Usuarios;

internal static class UsuarioMapper
{
    public static UsuarioResponse ToResponse(Usuario usuario) => new(
        usuario.Id,
        usuario.Nombre,
        usuario.Email,
        usuario.Username,
        EnumText.ToApiValue(usuario.Rol),
        usuario.EmpresaId,
        usuario.Empresa?.Nombre);
}