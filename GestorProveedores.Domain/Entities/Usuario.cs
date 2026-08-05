using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class Usuario : Entity<int>
{
    private Usuario()
    {
    }

    private Usuario(
        string nombre,
        string email,
        string username,
        string passwordHash,
        RolUsuario rol,
        int? empresaId)
    {
        Nombre = nombre;
        Email = email;
        Username = username;
        PasswordHash = passwordHash;
        Rol = rol;
        EmpresaId = empresaId;
        Activo = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public int? EmpresaId { get; private set; }
    public bool Activo { get; private set; }

    public Empresa? Empresa { get; private set; }

    public static Usuario Crear(
        string nombre,
        string email,
        string username,
        string passwordHash,
        RolUsuario rol,
        int? empresaId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainValidationException("usuario.nombre.requerido", "El nombre del usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("usuario.email.requerido", "El email del usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainValidationException("usuario.username.requerido", "El username del usuario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("usuario.password_hash.requerido", "El hash de contrasena es obligatorio.");
        }

        ValidarEmpresaPorRol(rol, empresaId);

        return new Usuario(nombre.Trim(), email.Trim(), username.Trim(), passwordHash, rol, empresaId);
    }

    public void Desactivar()
    {
        Activo = false;
        Touch();
    }

    private static void ValidarEmpresaPorRol(RolUsuario rol, int? empresaId)
    {
        var requiereEmpresa = rol is RolUsuario.Solicitante or RolUsuario.Aprobador;

        if (requiereEmpresa && empresaId is null)
        {
            throw new DomainValidationException("usuario.empresa.requerida", "El rol requiere una empresa asociada.");
        }

        if (!requiereEmpresa && empresaId is not null)
        {
            throw new DomainValidationException("usuario.empresa.no_permitida", "El rol no debe tener empresa asociada.");
        }
    }
}