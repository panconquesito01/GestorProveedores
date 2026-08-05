using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class Documento : Entity<int>
{
    private Documento()
    {
    }

    private Documento(
        int solicitudId,
        int? proveedorCandidatoId,
        TipoDocumento tipo,
        string nombreArchivo,
        string mimeType,
        byte[] contenido,
        int subidoPor)
    {
        SolicitudId = solicitudId;
        ProveedorCandidatoId = proveedorCandidatoId;
        Tipo = tipo;
        NombreArchivo = nombreArchivo;
        MimeType = mimeType;
        Contenido = contenido;
        SubidoPor = subidoPor;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public int SolicitudId { get; private set; }
    public int? ProveedorCandidatoId { get; private set; }
    public TipoDocumento Tipo { get; private set; }
    public string NombreArchivo { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public byte[] Contenido { get; private set; } = [];
    public int SubidoPor { get; private set; }

    public Solicitud Solicitud { get; private set; } = null!;
    public ProveedorCandidato? ProveedorCandidato { get; private set; }
    public Usuario UsuarioSubio { get; private set; } = null!;

    public static Documento Crear(
        int solicitudId,
        int? proveedorCandidatoId,
        TipoDocumento tipo,
        string nombreArchivo,
        string mimeType,
        byte[] contenido,
        int subidoPor)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            throw new DomainValidationException("documento.nombre.requerido", "El nombre del archivo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new DomainValidationException("documento.mime_type.requerido", "El tipo MIME es obligatorio.");
        }

        if (contenido.Length == 0)
        {
            throw new DomainValidationException("documento.contenido.requerido", "El contenido del documento es obligatorio.");
        }

        return new Documento(solicitudId, proveedorCandidatoId, tipo, nombreArchivo.Trim(), mimeType.Trim(), contenido, subidoPor);
    }
}