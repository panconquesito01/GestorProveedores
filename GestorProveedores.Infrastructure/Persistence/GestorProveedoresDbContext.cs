using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Persistence;

public sealed class GestorProveedoresDbContext(DbContextOptions<GestorProveedoresDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<AsignacionContador> AsignacionContadores => Set<AsignacionContador>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<ProveedorCandidato> ProveedoresCandidatos => Set<ProveedorCandidato>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<SolicitudHistorial> SolicitudHistorial => Set<SolicitudHistorial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestorProveedoresDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}