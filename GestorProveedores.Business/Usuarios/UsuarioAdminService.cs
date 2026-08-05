using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Business.Usuarios;

internal sealed class UsuarioAdminService(
    IUsuarioReadRepository usuarioReadRepository,
    IUsuarioRepository usuarioRepository,
    IPasswordHashGenerator passwordHashGenerator,
    IUnitOfWork unitOfWork) : IUsuarioAdminService
{
    public async Task<IReadOnlyList<UsuarioResponse>> ListarAsync(int? actorId, CancellationToken cancellationToken = default)
    {
        await ValidarSuperusuarioAsync(actorId, cancellationToken);

        var usuarios = await usuarioRepository.ListAsync(cancellationToken);

        return usuarios.Select(UsuarioMapper.ToResponse).ToList();
    }

    public async Task<UsuarioResponse> CrearAsync(UsuarioCreateRequest request, int? actorId, CancellationToken cancellationToken = default)
    {
        await ValidarSuperusuarioAsync(actorId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainValidationException("usuarios.password.requerido", "La contrasena es obligatoria.");
        }

        var rol = ParseRol(request.Rol);
        var existente = await usuarioRepository.GetByEmailOrUsernameAsync(request.Email.Trim(), request.Username.Trim(), cancellationToken);
        if (existente is not null)
        {
            throw new ConflictException("usuarios.duplicado", "Ya existe un usuario con el mismo email o username.");
        }

        var usuario = Usuario.Crear(
            request.Nombre,
            request.Email,
            request.Username,
            passwordHashGenerator.Generate(request.Password),
            rol,
            request.EmpresaId);

        usuarioRepository.Add(usuario);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UsuarioMapper.ToResponse(usuario);
    }

    private async Task<Usuario> ValidarSuperusuarioAsync(int? actorId, CancellationToken cancellationToken)
    {
        if (actorId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        var actor = await usuarioReadRepository.GetActiveByIdAsync(actorId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");

        if (actor.Rol is not RolUsuario.Superusuario)
        {
            throw new ForbiddenException("usuarios.admin.sin_permiso", "Solo el superusuario puede administrar usuarios.");
        }

        return actor;
    }

    private static RolUsuario ParseRol(string rol) => rol.Trim().ToLowerInvariant() switch
    {
        "superusuario" => RolUsuario.Superusuario,
        "solicitante" => RolUsuario.Solicitante,
        "auxiliar" => RolUsuario.Auxiliar,
        "analista" => RolUsuario.Analista,
        "aprobador" => RolUsuario.Aprobador,
        "contable" => RolUsuario.Contable,
        _ => throw new DomainValidationException("usuarios.rol.invalido", "Rol de usuario invalido.")
    };
}