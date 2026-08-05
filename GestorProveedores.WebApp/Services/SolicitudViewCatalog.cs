namespace GestorProveedores.WebApp.Services;

public static class SolicitudViewCatalog
{
    public static IReadOnlyList<SolicitudViewDefinition> GetForRole(string role) => role switch
    {
        "superusuario" =>
        [
            new("superusuario_todas", "Todas las solicitudes", "Vista global de todos los radicados", true)
        ],
        "solicitante" =>
        [
            new("solicitante_mias", "Mis solicitudes", "Radicadas por ti", true),
            new("solicitante_oc_revisar", "OC por revisar", "Ordenes de compra pendientes"),
            new("solicitante_facturas_revisar", "Facturas por revisar", "Facturas pendientes de concepto")
        ],
        "auxiliar" =>
        [
            new("auxiliar_paso1", "Revision inicial", "Solicitudes recien asignadas"),
            new("auxiliar_paso2", "Proveedores", "Gestion de proveedor ERP o nuevo"),
            new("auxiliar_paso4", "Orden de compra", "OC pendientes de cargar"),
            new("auxiliar_oc_devueltas", "OC devueltas", "Ordenes objetadas"),
            new("auxiliar_facturas_aprobadas", "Facturas aprobadas", "Listas para contabilidad"),
            new("auxiliar_facturas_objetadas", "Facturas objetadas", "Requieren reenvio")
        ],
        "analista" =>
        [
            new("analista_seleccion_proveedor", "Seleccion proveedor", "Candidatos por decidir"),
            new("analista_revision_anomalias", "Revision factura", "Anomalias y devoluciones")
        ],
        "aprobador" =>
        [
            new("aprobador_pendientes", "OC por aprobar", "Ordenes asignadas a tu aprobacion")
        ],
        "contable" =>
        [
            new("contable_facturas_validar", "Validacion contable", "Facturas listas para ERP")
        ],
        _ => []
    };
}