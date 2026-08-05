using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Repositories;

internal sealed class UsuarioRepository(GestorProveedoresDbContext dbContext) : IUsuarioRepository
{
    public async Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.Empresa)
            .OrderBy(usuario => usuario.Rol)
            .ThenBy(usuario => usuario.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Usuario?> GetByEmailOrUsernameAsync(string email, string username, CancellationToken cancellationToken = default) =>
        await dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Email == email || usuario.Username == username, cancellationToken);

    public void Add(Usuario usuario) => dbContext.Usuarios.Add(usuario);
}