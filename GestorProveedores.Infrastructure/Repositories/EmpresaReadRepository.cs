using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Repositories;

internal sealed class EmpresaReadRepository(GestorProveedoresDbContext dbContext) : IEmpresaReadRepository
{
    public async Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Empresas
            .AsNoTracking()
            .OrderBy(empresa => empresa.Nombre)
            .ToListAsync(cancellationToken);
}