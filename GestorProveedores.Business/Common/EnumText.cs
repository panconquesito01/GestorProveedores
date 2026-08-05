using GestorProveedores.Domain.Enums;

namespace GestorProveedores.Business.Common;

internal static class EnumText
{
    public static string ToApiValue(RolUsuario value) => value switch
    {
        RolUsuario.Superusuario => "superusuario",
        RolUsuario.Solicitante => "solicitante",
        RolUsuario.Auxiliar => "auxiliar",
        RolUsuario.Analista => "analista",
        RolUsuario.Aprobador => "aprobador",
        RolUsuario.Contable => "contable",
        _ => throw new InvalidOperationException($"Rol de usuario no soportado: {value}.")
    };

    public static string ToApiValue(EtapaSolicitud value) => value switch
    {
        EtapaSolicitud.RevisionAuxiliar => "revision_auxiliar",
        EtapaSolicitud.DevueltaSolicitante => "devuelta_solicitante",
        EtapaSolicitud.RevisionProveedores => "revision_proveedores",
        EtapaSolicitud.SeleccionProveedor => "seleccion_proveedor",
        EtapaSolicitud.CargaOrdenCompra => "carga_orden_compra",
        EtapaSolicitud.RevisionOcSolicitante => "revision_oc_solicitante",
        EtapaSolicitud.OcDevueltaAuxiliar => "oc_devuelta_auxiliar",
        EtapaSolicitud.RevisionOcAprobador => "revision_oc_aprobador",
        EtapaSolicitud.EnvioProveedor => "envio_proveedor",
        EtapaSolicitud.RevisionAnomalias => "revision_anomalias",
        EtapaSolicitud.RevisionFacturaSolicitante => "revision_factura_solicitante",
        EtapaSolicitud.FacturaDevueltaAnalista => "factura_devuelta_analista",
        EtapaSolicitud.FacturaAprobadaAuxiliar => "factura_aprobada_auxiliar",
        EtapaSolicitud.ValidacionContable => "validacion_contable",
        EtapaSolicitud.FacturaObjetadaContable => "factura_objetada_contable",
        EtapaSolicitud.Completada => "completada",
        _ => throw new InvalidOperationException($"Etapa de solicitud no soportada: {value}.")
    };

    public static string ToApiValue(EstadoSolicitud value) => value switch
    {
        EstadoSolicitud.EnProceso => "en_proceso",
        EstadoSolicitud.Devuelta => "devuelta",
        EstadoSolicitud.Completada => "completada",
        _ => throw new InvalidOperationException($"Estado de solicitud no soportado: {value}.")
    };

    public static string ToApiValue(OrigenProveedor value) => value switch
    {
        OrigenProveedor.ErpExistente => "erp_existente",
        OrigenProveedor.Nuevo => "nuevo",
        _ => throw new InvalidOperationException($"Origen de proveedor no soportado: {value}.")
    };

    public static string ToApiValue(TipoDocumento value) => value switch
    {
        TipoDocumento.Cotizacion => "cotizacion",
        TipoDocumento.CertificadoExistencia => "certificado_existencia",
        TipoDocumento.Rut => "rut",
        TipoDocumento.OrdenCompra => "orden_compra",
        TipoDocumento.Factura => "factura",
        TipoDocumento.SoporteContable => "soporte_contable",
        _ => throw new InvalidOperationException($"Tipo de documento no soportado: {value}.")
    };
}