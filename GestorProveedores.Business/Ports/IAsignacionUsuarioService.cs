using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;

namespace GestorProveedores.Business.Ports;

public interface IAsignacionUsuarioService
{
    Task<Usuario> AsignarSiguienteAsync(RolUsuario rol, CancellationToken cancellationToken = default);
}