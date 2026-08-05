using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class Solicitud : Entity<int>
{
    private readonly List<ProveedorCandidato> _proveedores = [];
    private readonly List<Documento> _documentos = [];
    private readonly List<SolicitudHistorial> _historial = [];

    private Solicitud()
    {
    }

    private Solicitud(
        string radicado,
        string titulo,
        string descripcion,
        string? frecuencia,
        int solicitanteId,
        int empresaId,
        int? aprobadorId,
        int auxiliarId)
    {
        Radicado = radicado;
        Titulo = titulo;
        Descripcion = descripcion;
        Frecuencia = frecuencia;
        SolicitanteId = solicitanteId;
        EmpresaId = empresaId;
        AprobadorId = aprobadorId;
        RequiereAprobacion = aprobadorId is not null;
        AuxiliarId = auxiliarId;
        Etapa = EtapaSolicitud.RevisionAuxiliar;
        Estado = EstadoSolicitud.EnProceso;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public string Radicado { get; private set; } = string.Empty;
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string? Frecuencia { get; private set; }
    public int SolicitanteId { get; private set; }
    public int EmpresaId { get; private set; }
    public int? AprobadorId { get; private set; }
    public bool RequiereAprobacion { get; private set; }
    public int? AuxiliarId { get; private set; }
    public int? AnalistaId { get; private set; }
    public OrigenProveedor? ProveedorOrigen { get; private set; }
    public EtapaSolicitud Etapa { get; private set; }
    public EstadoSolicitud Estado { get; private set; }

    public Usuario Solicitante { get; private set; } = null!;
    public Empresa Empresa { get; private set; } = null!;
    public Usuario? Aprobador { get; private set; }
    public Usuario? Auxiliar { get; private set; }
    public Usuario? Analista { get; private set; }
    public IReadOnlyCollection<ProveedorCandidato> Proveedores => _proveedores.AsReadOnly();
    public IReadOnlyCollection<Documento> Documentos => _documentos.AsReadOnly();
    public IReadOnlyCollection<SolicitudHistorial> Historial => _historial.AsReadOnly();

    public static Solicitud Crear(
        string radicado,
        string titulo,
        string descripcion,
        string? frecuencia,
        int solicitanteId,
        int empresaId,
        int? aprobadorId,
        int auxiliarId)
    {
        ValidarDatosBasicos(radicado, titulo, descripcion);

        if (solicitanteId <= 0)
        {
            throw new DomainValidationException("solicitud.solicitante.requerido", "El solicitante es obligatorio.");
        }

        if (empresaId <= 0)
        {
            throw new DomainValidationException("solicitud.empresa.requerida", "La empresa es obligatoria.");
        }

        if (auxiliarId <= 0)
        {
            throw new DomainValidationException("solicitud.auxiliar.requerido", "El auxiliar asignado es obligatorio.");
        }

        return new Solicitud(
            radicado.Trim(),
            titulo.Trim(),
            descripcion.Trim(),
            frecuencia?.Trim(),
            solicitanteId,
            empresaId,
            aprobadorId,
            auxiliarId);
    }

    public void ActualizarDatosBasicos(string titulo, string descripcion, string? frecuencia, int? aprobadorId)
    {
        ValidarDatosBasicos(Radicado, titulo, descripcion);

        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        Frecuencia = frecuencia?.Trim();
        AprobadorId = aprobadorId;
        RequiereAprobacion = aprobadorId is not null;
        Touch();
    }

    public void AsignarAnalista(int analistaId)
    {
        if (analistaId <= 0)
        {
            throw new DomainValidationException("solicitud.analista.invalido", "El analista asignado no es valido.");
        }

        AnalistaId = analistaId;
        Touch();
    }

    public void CambiarEtapa(EtapaSolicitud etapa, EstadoSolicitud estado)
    {
        Etapa = etapa;
        Estado = estado;
        Touch();
    }

    public void DefinirOrigenProveedor(OrigenProveedor origen)
    {
        ProveedorOrigen = origen;
        Touch();
    }

    private static void ValidarDatosBasicos(string radicado, string titulo, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(radicado))
        {
            throw new DomainValidationException("solicitud.radicado.requerido", "El radicado de la solicitud es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new DomainValidationException("solicitud.titulo.requerido", "El titulo de la solicitud es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new DomainValidationException("solicitud.descripcion.requerida", "La descripcion de la solicitud es obligatoria.");
        }
    }
}