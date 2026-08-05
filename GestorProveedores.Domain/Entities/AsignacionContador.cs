using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class AsignacionContador : Entity<RolUsuario>
{
    private AsignacionContador()
    {
    }

    private AsignacionContador(RolUsuario rol)
    {
        Id = rol;
        UltimoIndice = -1;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public RolUsuario Rol => Id;
    public int UltimoIndice { get; private set; }

    public static AsignacionContador Crear(RolUsuario rol)
    {
        if (rol is not (RolUsuario.Auxiliar or RolUsuario.Analista))
        {
            throw new DomainValidationException("asignacion.rol.invalido", "Solo se puede llevar contador para auxiliar o analista.");
        }

        return new AsignacionContador(rol);
    }

    public int Avanzar(int cantidadUsuarios)
    {
        if (cantidadUsuarios <= 0)
        {
            throw new DomainValidationException("asignacion.sin_usuarios", "No hay usuarios activos disponibles para asignar.");
        }

        UltimoIndice = (UltimoIndice + 1) % cantidadUsuarios;
        Touch();
        return UltimoIndice;
    }
}