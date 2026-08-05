using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Repositories;

internal sealed class DocumentoRepository(GestorProveedoresDbContext dbContext) : IDocumentoRepository
{
    public async Task<Documento?> GetByIdWithSolicitudAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.Documentos
            .AsNoTracking()
            .Include(documento => documento.Solicitud)
            .FirstOrDefaultAsync(documento => documento.Id == id, cancellationToken);
}