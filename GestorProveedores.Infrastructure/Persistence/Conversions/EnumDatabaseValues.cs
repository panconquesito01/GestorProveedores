using GestorProveedores.Domain.Enums;

namespace GestorProveedores.Infrastructure.Persistence.Conversions;

internal static class EnumDatabaseValues
{
    public static string ToDatabaseValue(RolUsuario value) => value switch
    {
        RolUsuario.Superusuario => "superusuario",
        RolUsuario.Solicitante => "solicitante",
        RolUsuario.Auxiliar => "auxiliar",
        RolUsuario.Analista => "analista",
        RolUsuario.Aprobador => "aprobador",
        RolUsuario.Contable => "contable",
        _ => throw new InvalidOperationException($"Rol de usuario no soportado: {value}.")
    };

    public static RolUsuario ToRolUsuario(string value) => value switch
    {
        "superusuario" => RolUsuario.Superusuario,
        "solicitante" => RolUsuario.Solicitante,
        "auxiliar" => RolUsuario.Auxiliar,
        "analista" => RolUsuario.Analista,
        "aprobador" => RolUsuario.Aprobador,
        "contable" => RolUsuario.Contable,
        _ => throw new InvalidOperationException($"Rol de usuario no soportado: {value}.")
    };

    public static string ToDatabaseValue(EstadoSolicitud value) => value switch
    {
        EstadoSolicitud.EnProceso => "en_proceso",
        EstadoSolicitud.Devuelta => "devuelta",
        EstadoSolicitud.Completada => "completada",
        _ => throw new InvalidOperationException($"Estado de solicitud no soportado: {value}.")
    };

    public static EstadoSolicitud ToEstadoSolicitud(string value) => value switch
    {
        "en_proceso" => EstadoSolicitud.EnProceso,
        "devuelta" => EstadoSolicitud.Devuelta,
        "completada" => EstadoSolicitud.Completada,
        _ => throw new InvalidOperationException($"Estado de solicitud no soportado: {value}.")
    };

    public static string ToDatabaseValue(OrigenProveedor value) => value switch
    {
        OrigenProveedor.ErpExistente => "erp_existente",
        OrigenProveedor.Nuevo => "nuevo",
        _ => throw new InvalidOperationException($"Origen de proveedor no soportado: {value}.")
    };

    public static OrigenProveedor ToOrigenProveedor(string value) => value switch
    {
        "erp_existente" => OrigenProveedor.ErpExistente,
        "nuevo" => OrigenProveedor.Nuevo,
        _ => throw new InvalidOperationException($"Origen de proveedor no soportado: {value}.")
    };

    public static string ToDatabaseValue(TipoDocumento value) => value switch
    {
        TipoDocumento.Cotizacion => "cotizacion",
        TipoDocumento.CertificadoExistencia => "certificado_existencia",
        TipoDocumento.Rut => "rut",
        TipoDocumento.OrdenCompra => "orden_compra",
        TipoDocumento.Factura => "factura",
        TipoDocumento.SoporteContable => "soporte_contable",
        _ => throw new InvalidOperationException($"Tipo de documento no soportado: {value}.")
    };

    public static TipoDocumento ToTipoDocumento(string value) => value switch
    {
        "cotizacion" => TipoDocumento.Cotizacion,
        "certificado_existencia" => TipoDocumento.CertificadoExistencia,
        "rut" => TipoDocumento.Rut,
        "orden_compra" => TipoDocumento.OrdenCompra,
        "factura" => TipoDocumento.Factura,
        "soporte_contable" => TipoDocumento.SoporteContable,
        _ => throw new InvalidOperationException($"Tipo de documento no soportado: {value}.")
    };

    public static string ToDatabaseValue(EtapaSolicitud value) => value switch
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

    public static EtapaSolicitud ToEtapaSolicitud(string value) => value switch
    {
        "revision_auxiliar" => EtapaSolicitud.RevisionAuxiliar,
        "devuelta_solicitante" => EtapaSolicitud.DevueltaSolicitante,
        "revision_proveedores" => EtapaSolicitud.RevisionProveedores,
        "seleccion_proveedor" => EtapaSolicitud.SeleccionProveedor,
        "carga_orden_compra" => EtapaSolicitud.CargaOrdenCompra,
        "revision_oc_solicitante" => EtapaSolicitud.RevisionOcSolicitante,
        "oc_devuelta_auxiliar" => EtapaSolicitud.OcDevueltaAuxiliar,
        "revision_oc_aprobador" => EtapaSolicitud.RevisionOcAprobador,
        "envio_proveedor" => EtapaSolicitud.EnvioProveedor,
        "revision_anomalias" => EtapaSolicitud.RevisionAnomalias,
        "revision_factura_solicitante" => EtapaSolicitud.RevisionFacturaSolicitante,
        "factura_devuelta_analista" => EtapaSolicitud.FacturaDevueltaAnalista,
        "factura_aprobada_auxiliar" => EtapaSolicitud.FacturaAprobadaAuxiliar,
        "validacion_contable" => EtapaSolicitud.ValidacionContable,
        "factura_objetada_contable" => EtapaSolicitud.FacturaObjetadaContable,
        "completada" => EtapaSolicitud.Completada,
        _ => throw new InvalidOperationException($"Etapa de solicitud no soportada: {value}.")
    };
}