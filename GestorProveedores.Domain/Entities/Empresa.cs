using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class Empresa : Entity<int>
{
    private Empresa()
    {
    }

    private Empresa(string nombre, string nit)
    {
        Nombre = nombre;
        Nit = nit;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Nit { get; private set; } = string.Empty;

    public static Empresa Crear(string nombre, string nit)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainValidationException("empresa.nombre.requerido", "El nombre de la empresa es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nit))
        {
            throw new DomainValidationException("empresa.nit.requerido", "El NIT de la empresa es obligatorio.");
        }

        return new Empresa(nombre.Trim(), nit.Trim());
    }
}