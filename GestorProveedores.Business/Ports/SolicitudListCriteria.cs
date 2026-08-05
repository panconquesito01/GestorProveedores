using GestorProveedores.Domain.Enums;

namespace GestorProveedores.Business.Ports;

public sealed record SolicitudListCriteria(
    IReadOnlyCollection<EtapaSolicitud>? Etapas,
    SolicitudAsignacionCampo AsignacionCampo,
    int UsuarioId,
    string? SolicitanteNombre,
    int? EmpresaId,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta);