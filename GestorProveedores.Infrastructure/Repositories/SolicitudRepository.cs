using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Repositories;

internal sealed class SolicitudRepository(GestorProveedoresDbContext dbContext) : ISolicitudRepository
{
    public async Task<IReadOnlyList<Solicitud>> ListAsync(
        SolicitudListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Solicitudes
            .AsNoTracking()
            .Include(solicitud => solicitud.Empresa)
            .Include(solicitud => solicitud.Solicitante)
            .AsQueryable();

        if (criteria.Etapas is not null)
        {
            query = query.Where(solicitud => criteria.Etapas.Contains(solicitud.Etapa));
        }

        query = criteria.AsignacionCampo switch
        {
            SolicitudAsignacionCampo.Solicitante => query.Where(solicitud => solicitud.SolicitanteId == criteria.UsuarioId),
            SolicitudAsignacionCampo.Auxiliar => query.Where(solicitud => solicitud.AuxiliarId == criteria.UsuarioId),
            SolicitudAsignacionCampo.Analista => query.Where(solicitud => solicitud.AnalistaId == criteria.UsuarioId),
            SolicitudAsignacionCampo.Aprobador => query.Where(solicitud => solicitud.AprobadorId == criteria.UsuarioId),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(criteria.SolicitanteNombre))
        {
            var pattern = $"%{criteria.SolicitanteNombre.Trim()}%";
            query = query.Where(solicitud => EF.Functions.Like(solicitud.Solicitante.Nombre, pattern));
        }

        if (criteria.EmpresaId is not null)
        {
            query = query.Where(solicitud => solicitud.EmpresaId == criteria.EmpresaId.Value);
        }

        if (criteria.FechaDesde is not null)
        {
            var fechaDesde = new DateTimeOffset(criteria.FechaDesde.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(solicitud => solicitud.CreatedAt >= fechaDesde);
        }

        if (criteria.FechaHasta is not null)
        {
            var fechaHastaExclusiva = new DateTimeOffset(criteria.FechaHasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(solicitud => solicitud.CreatedAt < fechaHastaExclusiva);
        }

        return await query
            .OrderByDescending(solicitud => solicitud.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Solicitud?> GetDetailByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.Solicitudes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(solicitud => solicitud.Solicitante)
            .Include(solicitud => solicitud.Empresa)
            .Include(solicitud => solicitud.Aprobador)
            .Include(solicitud => solicitud.Auxiliar)
            .Include(solicitud => solicitud.Analista)
            .Include(solicitud => solicitud.Proveedores)
            .Include(solicitud => solicitud.Documentos)
            .Include(solicitud => solicitud.Historial)
                .ThenInclude(historial => historial.Actor)
            .FirstOrDefaultAsync(solicitud => solicitud.Id == id, cancellationToken);

    public async Task<Solicitud?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        await dbContext.Solicitudes
            .Include(solicitud => solicitud.Proveedores)
            .FirstOrDefaultAsync(solicitud => solicitud.Id == id, cancellationToken);

    public void Add(Solicitud solicitud) => dbContext.Solicitudes.Add(solicitud);

    public void AddProveedor(ProveedorCandidato proveedor) => dbContext.ProveedoresCandidatos.Add(proveedor);

    public void RemoveProveedor(ProveedorCandidato proveedor) => dbContext.ProveedoresCandidatos.Remove(proveedor);

    public void AddDocumento(Documento documento) => dbContext.Documentos.Add(documento);

    public void AddHistorial(SolicitudHistorial historial) => dbContext.SolicitudHistorial.Add(historial);
}