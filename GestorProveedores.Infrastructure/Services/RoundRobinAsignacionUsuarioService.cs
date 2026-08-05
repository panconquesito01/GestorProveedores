using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Services;

internal sealed class RoundRobinAsignacionUsuarioService(GestorProveedoresDbContext dbContext) : IAsignacionUsuarioService
{
    public async Task<Usuario> AsignarSiguienteAsync(RolUsuario rol, CancellationToken cancellationToken = default)
    {
        var usuarios = await dbContext.Usuarios
            .Where(usuario => usuario.Activo && usuario.Rol == rol)
            .OrderBy(usuario => usuario.Id)
            .ToListAsync(cancellationToken);

        if (usuarios.Count == 0)
        {
            throw new DomainValidationException("asignacion.sin_usuarios", $"No hay usuarios activos con rol {rol} para asignar.");
        }

        var contador = await dbContext.AsignacionContadores
            .FirstOrDefaultAsync(asignacion => asignacion.Id == rol, cancellationToken);

        if (contador is null)
        {
            contador = AsignacionContador.Crear(rol);
            dbContext.AsignacionContadores.Add(contador);
        }

        var siguienteIndice = contador.Avanzar(usuarios.Count);

        return usuarios[siguienteIndice];
    }
}