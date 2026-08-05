using GestorProveedores.Domain.Common;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Domain.Entities;

public sealed class ProveedorCandidato : Entity<int>
{
    private ProveedorCandidato()
    {
    }

    private ProveedorCandidato(
        int solicitudId,
        short orden,
        OrigenProveedor origen,
        string nombre,
        string? nit,
        string? identificadorErp,
        string? correoContacto,
        string? telefonoContacto,
        bool validado,
        bool creadoEnErp,
        bool seleccionado)
    {
        SolicitudId = solicitudId;
        Orden = orden;
        Origen = origen;
        Nombre = nombre;
        Nit = nit;
        IdentificadorErp = identificadorErp;
        CorreoContacto = correoContacto;
        TelefonoContacto = telefonoContacto;
        Validado = validado;
        CreadoEnErp = creadoEnErp;
        Seleccionado = seleccionado;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public int SolicitudId { get; private set; }
    public short Orden { get; private set; }
    public OrigenProveedor Origen { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Nit { get; private set; }
    public string? IdentificadorErp { get; private set; }
    public string? CorreoContacto { get; private set; }
    public string? TelefonoContacto { get; private set; }
    public bool Validado { get; private set; }
    public bool CreadoEnErp { get; private set; }
    public bool Seleccionado { get; private set; }

    public Solicitud Solicitud { get; private set; } = null!;

    public static ProveedorCandidato CrearErpExistente(
        int solicitudId,
        string nombre,
        string? nit,
        string? identificadorErp,
        string? correoContacto,
        string? telefonoContacto)
    {
        ValidarNombre(nombre);

        return new ProveedorCandidato(
            solicitudId,
            1,
            OrigenProveedor.ErpExistente,
            nombre.Trim(),
            nit,
            identificadorErp,
            correoContacto,
            telefonoContacto,
            validado: true,
            creadoEnErp: true,
            seleccionado: true);
    }

    public static ProveedorCandidato CrearNuevo(
        int solicitudId,
        short orden,
        string nombre,
        string? nit,
        string? correoContacto,
        string? telefonoContacto,
        bool validado)
    {
        ValidarNombre(nombre);

        if (orden is < 1 or > 3)
        {
            throw new DomainValidationException("proveedor.orden.invalido", "El orden del proveedor debe estar entre 1 y 3.");
        }

        return new ProveedorCandidato(
            solicitudId,
            orden,
            OrigenProveedor.Nuevo,
            nombre.Trim(),
            nit,
            identificadorErp: null,
            correoContacto,
            telefonoContacto,
            validado,
            creadoEnErp: false,
            seleccionado: false);
    }

    public void ActualizarNuevo(
        string nombre,
        string? nit,
        string? correoContacto,
        string? telefonoContacto,
        bool validado)
    {
        if (Origen is not OrigenProveedor.Nuevo)
        {
            throw new DomainValidationException("proveedor.origen.invalido", "Solo se pueden actualizar proveedores nuevos en este flujo.");
        }

        ValidarNombre(nombre);

        Nombre = nombre.Trim();
        Nit = nit;
        CorreoContacto = correoContacto;
        TelefonoContacto = telefonoContacto;
        Validado = validado;

        if (!Validado)
        {
            CreadoEnErp = false;
        }

        Touch();
    }

    public void MarcarCreadoEnErp(bool creadoEnErp)
    {
        if (!Validado)
        {
            throw new DomainValidationException("proveedor.no_validado", "Solo se puede marcar creado en ERP un proveedor validado.");
        }

        CreadoEnErp = creadoEnErp;
        Touch();
    }

    public void Seleccionar()
    {
        if (!Validado)
        {
            throw new DomainValidationException("proveedor.no_validado", "Solo se puede seleccionar un proveedor validado.");
        }

        Seleccionado = true;
        Touch();
    }

    public void QuitarSeleccion()
    {
        Seleccionado = false;
        Touch();
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainValidationException("proveedor.nombre.requerido", "El nombre del proveedor es obligatorio.");
        }
    }
}