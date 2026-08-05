using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class SolicitudHistorial : Entity<int>
{
    private SolicitudHistorial()
    {
    }

    private SolicitudHistorial(int solicitudId, EtapaSolicitud etapa, string accion, int? actorId, string? comentario)
    {
        SolicitudId = solicitudId;
        Etapa = etapa;
        Accion = accion;
        ActorId = actorId;
        Comentario = comentario;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public int SolicitudId { get; private set; }
    public EtapaSolicitud Etapa { get; private set; }
    public string Accion { get; private set; } = string.Empty;
    public int? ActorId { get; private set; }
    public string? Comentario { get; private set; }

    public Solicitud Solicitud { get; private set; } = null!;
    public Usuario? Actor { get; private set; }

    public static SolicitudHistorial Crear(
        int solicitudId,
        EtapaSolicitud etapa,
        string accion,
        int? actorId,
        string? comentario = null)
    {
        if (string.IsNullOrWhiteSpace(accion))
        {
            throw new DomainValidationException("historial.accion.requerida", "La accion de historial es obligatoria.");
        }

        return new SolicitudHistorial(solicitudId, etapa, accion.Trim(), actorId, comentario?.Trim());
    }
}