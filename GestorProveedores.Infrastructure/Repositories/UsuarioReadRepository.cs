using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Repositories;

internal sealed class UsuarioReadRepository(GestorProveedoresDbContext dbContext) : IUsuarioReadRepository
{
    public async Task<Usuario?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Empresa)
            .FirstOrDefaultAsync(usuario => usuario.Id == id && usuario.Activo, cancellationToken);

    public async Task<Usuario?> GetActiveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Empresa)
            .FirstOrDefaultAsync(
                usuario => usuario.Activo && (usuario.Username == identifier || usuario.Email == identifier),
                cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListActiveApproversByEmpresaAsync(int empresaId, CancellationToken cancellationToken = default) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Activo && usuario.Rol == RolUsuario.Aprobador && usuario.EmpresaId == empresaId)
            .OrderBy(usuario => usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListActiveByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.Activo && usuario.Rol == rol)
            .OrderBy(usuario => usuario.Id)
            .ToListAsync(cancellationToken);
}