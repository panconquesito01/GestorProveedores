using GestorProveedores.Business.Ports;
using GestorProveedores.Business.Solicitudes;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Business.Workflow;

internal sealed class WorkflowService(
    IUsuarioReadRepository usuarioReadRepository,
    ISolicitudRepository solicitudRepository,
    IAsignacionUsuarioService asignacionUsuarioService,
    IUnitOfWork unitOfWork) : IWorkflowService
{
    private static readonly IReadOnlyDictionary<string, string> MotivosObjecionContable = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["creacion_tercero"] = "Creacion erronea del tercero",
        ["retenciones"] = "Retenciones",
        ["tarifas_impuestos"] = "Tarifas de impuestos"
    };

    public async Task<SolicitudDetalleResponse> DevolverPaso1Async(
        int id,
        ComentarioRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso1Async(id, auxiliar.Id, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Comentario))
        {
            throw new DomainValidationException(
                "workflow.paso1.comentario_requerido",
                "El comentario es obligatorio para devolver la solicitud.");
        }

        solicitud.CambiarEtapa(EtapaSolicitud.DevueltaSolicitante, EstadoSolicitud.Devuelta);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Solicitud devuelta al solicitante",
            auxiliar.Id,
            request.Comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> AvanzarPaso1Async(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso1Async(id, auxiliar.Id, cancellationToken);

        solicitud.CambiarEtapa(EtapaSolicitud.RevisionProveedores, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Solicitud aprobada en revision inicial, pasa a revision de proveedores",
            auxiliar.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> RegistrarProveedorErpAsync(
        int id,
        ProveedorErpRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso2Async(id, auxiliar.Id, cancellationToken);

        foreach (var proveedor in solicitud.Proveedores.ToList())
        {
            solicitudRepository.RemoveProveedor(proveedor);
        }

        var proveedorErp = ProveedorCandidato.CrearErpExistente(
            solicitud.Id,
            request.Nombre,
            request.Nit,
            request.IdentificadorErp,
            request.CorreoContacto,
            request.TelefonoContacto);

        solicitudRepository.AddProveedor(proveedorErp);
        solicitud.DefinirOrigenProveedor(OrigenProveedor.ErpExistente);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            $"Registrado proveedor existente en ERP: {proveedorErp.Nombre}",
            auxiliar.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> AvanzarProveedorErpAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso2Async(id, auxiliar.Id, cancellationToken);

        if (solicitud.ProveedorOrigen is not OrigenProveedor.ErpExistente ||
            !solicitud.Proveedores.Any(proveedor => proveedor.Origen is OrigenProveedor.ErpExistente))
        {
            throw new DomainValidationException(
                "workflow.paso2.proveedor_erp_requerido",
                "Debes registrar el proveedor existente en el ERP antes de continuar.");
        }

        solicitud.CambiarEtapa(EtapaSolicitud.CargaOrdenCompra, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Proveedor ERP confirmado, pasa a cargar orden de compra",
            auxiliar.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> GuardarProveedoresNuevosAsync(
        int id,
        ProveedoresNuevosRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso2Async(id, auxiliar.Id, cancellationToken);

        if (request.Candidatos is null || request.Candidatos.Count > 3)
        {
            throw new DomainValidationException(
                "workflow.paso2.candidatos_invalidos",
                "La solicitud debe incluir hasta 3 proveedores candidatos.");
        }

        var proveedoresNuevos = solicitud.Proveedores
            .Where(proveedor => proveedor.Origen is OrigenProveedor.Nuevo)
            .ToDictionary(proveedor => proveedor.Orden);

        foreach (var proveedor in solicitud.Proveedores.Where(proveedor => proveedor.Origen is OrigenProveedor.ErpExistente).ToList())
        {
            solicitudRepository.RemoveProveedor(proveedor);
        }

        for (var index = 0; index < request.Candidatos.Count; index++)
        {
            var orden = (short)(index + 1);
            var candidato = request.Candidatos[index];

            if (proveedoresNuevos.TryGetValue(orden, out var proveedor))
            {
                proveedor.ActualizarNuevo(
                    candidato.Nombre,
                    candidato.Nit,
                    candidato.CorreoContacto,
                    candidato.TelefonoContacto,
                    candidato.Validado);

                continue;
            }

            solicitudRepository.AddProveedor(ProveedorCandidato.CrearNuevo(
                solicitud.Id,
                orden,
                candidato.Nombre,
                candidato.Nit,
                candidato.CorreoContacto,
                candidato.TelefonoContacto,
                candidato.Validado));
        }

        solicitud.DefinirOrigenProveedor(OrigenProveedor.Nuevo);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Proveedores candidatos registrados/actualizados",
            auxiliar.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> MarcarProveedorCreadoEnErpAsync(
        int id,
        int proveedorId,
        CreadoEnErpRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso2Async(id, auxiliar.Id, cancellationToken);
        var proveedor = solicitud.Proveedores.FirstOrDefault(proveedor => proveedor.Id == proveedorId)
            ?? throw new DomainValidationException(
                "workflow.paso2.proveedor_no_encontrado",
                "Proveedor no encontrado en esta solicitud.");

        proveedor.MarcarCreadoEnErp(request.CreadoEnErp);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> AvanzarProveedoresNuevosAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso2Async(id, auxiliar.Id, cancellationToken);

        var proveedoresValidados = solicitud.Proveedores
            .Where(proveedor => proveedor.Origen is OrigenProveedor.Nuevo && proveedor.Validado)
            .ToList();

        if (proveedoresValidados.Count < 2)
        {
            throw new DomainValidationException(
                "workflow.paso2.proveedores_validados_insuficientes",
                "Se requieren al menos 2 de los 3 proveedores marcados como validados.");
        }

        if (proveedoresValidados.Any(proveedor => !proveedor.CreadoEnErp))
        {
            throw new DomainValidationException(
                "workflow.paso2.proveedores_no_creados_en_erp",
                "Todos los proveedores validados deben estar marcados como ya creado en ERP.");
        }

        if (solicitud.AnalistaId is null)
        {
            var analista = await asignacionUsuarioService.AsignarSiguienteAsync(RolUsuario.Analista, cancellationToken);
            solicitud.AsignarAnalista(analista.Id);
        }

        solicitud.CambiarEtapa(EtapaSolicitud.SeleccionProveedor, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Proveedores validados y creados en ERP, pasa a seleccion de proveedor",
            auxiliar.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> SeleccionarProveedorAsync(
        int id,
        SeleccionarProveedorRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var analista = await ObtenerAnalistaAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudPaso3Async(id, analista.Id, cancellationToken);
        var proveedorSeleccionado = solicitud.Proveedores.FirstOrDefault(proveedor => proveedor.Id == request.ProveedorId);

        if (proveedorSeleccionado is null || !proveedorSeleccionado.Validado)
        {
            throw new DomainValidationException(
                "workflow.paso3.proveedor_invalido",
                "Solo puedes seleccionar un proveedor validado de esta solicitud.");
        }

        foreach (var proveedor in solicitud.Proveedores)
        {
            proveedor.QuitarSeleccion();
        }

        proveedorSeleccionado.Seleccionar();
        solicitud.CambiarEtapa(EtapaSolicitud.CargaOrdenCompra, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            $"Proveedor seleccionado: {proveedorSeleccionado.Nombre}",
            analista.Id,
            request.Comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> SubirDocumentoProveedorAsync(
        int id,
        int proveedorId,
        string tipo,
        ArchivoWorkflowRequest archivo,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudAsignadaAsync(
            id,
            auxiliar.Id,
            solicitud => solicitud.AuxiliarId,
            "workflow.auxiliar.no_asignado",
            "No eres el auxiliar asignado a esta solicitud.",
            cancellationToken);

        if (!solicitud.Proveedores.Any(proveedor => proveedor.Id == proveedorId))
        {
            throw new NotFoundException("workflow.proveedor.no_encontrado", "Proveedor no encontrado en esta solicitud.");
        }

        solicitudRepository.AddDocumento(Documento.Crear(
            solicitud.Id,
            proveedorId,
            ObtenerTipoDocumento(tipo),
            archivo.NombreArchivo,
            archivo.MimeType,
            archivo.Contenido,
            auxiliar.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> CargarOrdenCompraAsync(
        int id,
        ArchivoWorkflowRequest archivo,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.CargaOrdenCompra, EtapaSolicitud.OcDevueltaAuxiliar],
            "workflow.paso4.etapa_invalida",
            "La solicitud no esta lista para cargar orden de compra.",
            cancellationToken);

        ValidarActorAsignado(solicitud.AuxiliarId, auxiliar.Id, "workflow.auxiliar.no_asignado", "No eres el auxiliar asignado a esta solicitud.");

        solicitudRepository.AddDocumento(Documento.Crear(
            solicitud.Id,
            proveedorCandidatoId: null,
            TipoDocumento.OrdenCompra,
            archivo.NombreArchivo,
            archivo.MimeType,
            archivo.Contenido,
            auxiliar.Id));

        solicitud.CambiarEtapa(EtapaSolicitud.RevisionOcSolicitante, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Orden de compra cargada, enviada al solicitante para revision",
            auxiliar.Id,
            comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> RevisarOrdenCompraSolicitanteAsync(
        int id,
        DecisionRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var solicitante = await ObtenerSolicitanteAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.RevisionOcSolicitante],
            "workflow.paso5.solicitante.etapa_invalida",
            "La solicitud no esta en revision de orden de compra por el solicitante.",
            cancellationToken);

        ValidarActorAsignado(solicitud.SolicitanteId, solicitante.Id, "workflow.solicitante.no_asignado", "No eres el solicitante asignado a esta solicitud.");

        if (!request.Aprobado)
        {
            RequerirComentario(request.Comentario, "workflow.paso5.solicitante.comentario_requerido", "El comentario es obligatorio para objetar la orden de compra.");
            solicitud.CambiarEtapa(EtapaSolicitud.OcDevueltaAuxiliar, EstadoSolicitud.Devuelta);
            solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
                solicitud.Id,
                solicitud.Etapa,
                "Orden de compra objetada por el solicitante",
                solicitante.Id,
                request.Comentario));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
        }

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Orden de compra aprobada por el solicitante",
            solicitante.Id,
            request.Comentario));

        if (solicitud.RequiereAprobacion)
        {
            solicitud.CambiarEtapa(EtapaSolicitud.RevisionOcAprobador, EstadoSolicitud.EnProceso);
        }
        else
        {
            await EjecutarEnvioProveedorAsync(solicitud, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> RevisarOrdenCompraAprobadorAsync(
        int id,
        DecisionRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var aprobador = await ObtenerAprobadorAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.RevisionOcAprobador],
            "workflow.paso5.aprobador.etapa_invalida",
            "La solicitud no esta en revision de orden de compra por aprobador.",
            cancellationToken);

        ValidarActorAsignado(solicitud.AprobadorId, aprobador.Id, "workflow.aprobador.no_asignado", "No eres el aprobador asignado a esta solicitud.");

        if (!request.Aprobado)
        {
            RequerirComentario(request.Comentario, "workflow.paso5.aprobador.comentario_requerido", "El comentario es obligatorio para rechazar la orden de compra.");
            solicitud.CambiarEtapa(EtapaSolicitud.OcDevueltaAuxiliar, EstadoSolicitud.Devuelta);
            solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
                solicitud.Id,
                solicitud.Etapa,
                "Orden de compra rechazada por el aprobador",
                aprobador.Id,
                request.Comentario));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
        }

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Orden de compra aprobada por el aprobador",
            aprobador.Id,
            request.Comentario));

        await EjecutarEnvioProveedorAsync(solicitud, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> CargarFacturaAsync(
        int id,
        ArchivoWorkflowRequest archivo,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var analista = await ObtenerAnalistaAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.RevisionAnomalias, EtapaSolicitud.FacturaDevueltaAnalista],
            "workflow.paso6.etapa_invalida",
            "La solicitud no esta lista para cargar factura.",
            cancellationToken);

        ValidarActorAsignado(solicitud.AnalistaId, analista.Id, "workflow.analista.no_asignado", "No eres el analista asignado a esta solicitud.");

        solicitudRepository.AddDocumento(Documento.Crear(
            solicitud.Id,
            proveedorCandidatoId: null,
            TipoDocumento.Factura,
            archivo.NombreArchivo,
            archivo.MimeType,
            archivo.Contenido,
            analista.Id));

        solicitud.CambiarEtapa(EtapaSolicitud.RevisionFacturaSolicitante, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Factura remitida al solicitante para revision",
            analista.Id,
            comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> RevisarFacturaSolicitanteAsync(
        int id,
        DecisionRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var solicitante = await ObtenerSolicitanteAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.RevisionFacturaSolicitante],
            "workflow.paso7.etapa_invalida",
            "La solicitud no esta en revision de factura por el solicitante.",
            cancellationToken);

        ValidarActorAsignado(solicitud.SolicitanteId, solicitante.Id, "workflow.solicitante.no_asignado", "No eres el solicitante asignado a esta solicitud.");

        if (!request.Aprobado)
        {
            RequerirComentario(request.Comentario, "workflow.paso7.comentario_requerido", "El motivo del rechazo es obligatorio.");
            solicitud.CambiarEtapa(EtapaSolicitud.FacturaDevueltaAnalista, EstadoSolicitud.Devuelta);
            solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
                solicitud.Id,
                solicitud.Etapa,
                "Factura rechazada por el solicitante",
                solicitante.Id,
                request.Comentario));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
        }

        solicitud.CambiarEtapa(EtapaSolicitud.FacturaAprobadaAuxiliar, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Factura aprobada por el solicitante",
            solicitante.Id,
            request.Comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> EnviarContabilidadAsync(
        int id,
        IReadOnlyList<ArchivoWorkflowRequest> soportes,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.FacturaAprobadaAuxiliar],
            "workflow.paso8.etapa_invalida",
            "La solicitud no esta lista para enviar soportes a contabilidad.",
            cancellationToken);

        ValidarActorAsignado(solicitud.AuxiliarId, auxiliar.Id, "workflow.auxiliar.no_asignado", "No eres el auxiliar asignado a esta solicitud.");

        foreach (var soporte in soportes)
        {
            solicitudRepository.AddDocumento(Documento.Crear(
                solicitud.Id,
                proveedorCandidatoId: null,
                TipoDocumento.SoporteContable,
                soporte.NombreArchivo,
                soporte.MimeType,
                soporte.Contenido,
                auxiliar.Id));
        }

        solicitud.CambiarEtapa(EtapaSolicitud.ValidacionContable, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Factura y soportes enviados a contabilidad",
            auxiliar.Id,
            comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> MarcarConformeContabilidadAsync(
        int id,
        ConformeRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contable = await ObtenerContableAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.ValidacionContable],
            "workflow.paso9.etapa_invalida",
            "La solicitud no esta en validacion contable.",
            cancellationToken);

        if (!request.ConfirmacionErp)
        {
            throw new DomainValidationException(
                "workflow.paso9.confirmacion_erp_requerida",
                "Debes confirmar que ya realizaste las gestiones en el ERP para finalizar.");
        }

        solicitud.CambiarEtapa(EtapaSolicitud.Completada, EstadoSolicitud.Completada);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Proceso finalizado por contabilidad (gestion en ERP confirmada)",
            contable.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> ObjetarContabilidadAsync(
        int id,
        ObjetarContableRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contable = await ObtenerContableAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.ValidacionContable],
            "workflow.paso9.etapa_invalida",
            "La solicitud no esta en validacion contable.",
            cancellationToken);

        if (!MotivosObjecionContable.TryGetValue(request.Motivo, out var etiqueta))
        {
            throw new DomainValidationException("workflow.paso9.motivo_invalido", "Motivo de objecion invalido.");
        }

        solicitud.CambiarEtapa(EtapaSolicitud.FacturaObjetadaContable, EstadoSolicitud.Devuelta);

        var accion = $"Objecion de contabilidad: {etiqueta}";
        if (!string.IsNullOrWhiteSpace(request.Comentario))
        {
            accion = $"{accion} - {request.Comentario.Trim()}";
        }

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            accion,
            contable.Id,
            request.Comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleResponse> ReenviarFacturaObjetadaAsync(
        int id,
        ArchivoWorkflowRequest archivo,
        string? comentario,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var auxiliar = await ObtenerAuxiliarAsync(usuarioId, cancellationToken);
        var solicitud = await ObtenerSolicitudEnEtapasAsync(
            id,
            [EtapaSolicitud.FacturaObjetadaContable],
            "workflow.paso9.reenvio.etapa_invalida",
            "La solicitud no tiene una factura objetada por contabilidad.",
            cancellationToken);

        ValidarActorAsignado(solicitud.AuxiliarId, auxiliar.Id, "workflow.auxiliar.no_asignado", "No eres el auxiliar asignado a esta solicitud.");

        solicitudRepository.AddDocumento(Documento.Crear(
            solicitud.Id,
            proveedorCandidatoId: null,
            TipoDocumento.Factura,
            archivo.NombreArchivo,
            archivo.MimeType,
            archivo.Contenido,
            auxiliar.Id));

        solicitud.CambiarEtapa(EtapaSolicitud.RevisionFacturaSolicitante, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Nueva factura cargada tras objecion de contabilidad, enviada al solicitante",
            auxiliar.Id,
            comentario));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ObtenerDetalleActualizadoAsync(solicitud.Id, cancellationToken);
    }

    private async Task<Usuario> ObtenerAuxiliarAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");

        if (usuario.Rol is not RolUsuario.Auxiliar)
        {
            throw new ForbiddenException("workflow.auxiliar.rol_invalido", "Solo el rol auxiliar puede ejecutar esta accion.");
        }

        return usuario;
    }

    private async Task<Usuario> ObtenerSolicitanteAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");

        if (usuario.Rol is not RolUsuario.Solicitante)
        {
            throw new ForbiddenException("workflow.solicitante.rol_invalido", "Solo el rol solicitante puede ejecutar esta accion.");
        }

        return usuario;
    }

    private async Task<Usuario> ObtenerAprobadorAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");

        if (usuario.Rol is not RolUsuario.Aprobador)
        {
            throw new ForbiddenException("workflow.aprobador.rol_invalido", "Solo el rol aprobador puede ejecutar esta accion.");
        }

        return usuario;
    }

    private async Task<Usuario> ObtenerContableAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");

        if (usuario.Rol is not RolUsuario.Contable)
        {
            throw new ForbiddenException("workflow.contable.rol_invalido", "Solo el rol contable puede ejecutar esta accion.");
        }

        return usuario;
    }

    private async Task<Usuario> ObtenerAnalistaAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");

        if (usuario.Rol is not RolUsuario.Analista)
        {
            throw new ForbiddenException("workflow.analista.rol_invalido", "Solo el rol analista puede ejecutar esta accion.");
        }

        return usuario;
    }

    private async Task<Solicitud> ObtenerSolicitudPaso1Async(int id, int auxiliarId, CancellationToken cancellationToken)
    {
        var solicitud = await solicitudRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        if (solicitud.Etapa is not EtapaSolicitud.RevisionAuxiliar)
        {
            throw new DomainValidationException(
                "workflow.paso1.etapa_invalida",
                $"La solicitud esta en la etapa '{solicitud.Etapa}', no se puede procesar en el paso 1.");
        }

        if (solicitud.AuxiliarId != auxiliarId)
        {
            throw new ForbiddenException("workflow.auxiliar.no_asignado", "No eres el auxiliar asignado a esta solicitud.");
        }

        return solicitud;
    }

    private async Task<Solicitud> ObtenerSolicitudPaso2Async(int id, int auxiliarId, CancellationToken cancellationToken)
    {
        var solicitud = await solicitudRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        if (solicitud.Etapa is not EtapaSolicitud.RevisionProveedores)
        {
            throw new DomainValidationException(
                "workflow.paso2.etapa_invalida",
                $"La solicitud esta en la etapa '{solicitud.Etapa}', no se puede procesar en el paso 2.");
        }

        if (solicitud.AuxiliarId != auxiliarId)
        {
            throw new ForbiddenException("workflow.auxiliar.no_asignado", "No eres el auxiliar asignado a esta solicitud.");
        }

        return solicitud;
    }

    private async Task<Solicitud> ObtenerSolicitudPaso3Async(int id, int analistaId, CancellationToken cancellationToken)
    {
        var solicitud = await solicitudRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        if (solicitud.Etapa is not EtapaSolicitud.SeleccionProveedor)
        {
            throw new DomainValidationException(
                "workflow.paso3.etapa_invalida",
                $"La solicitud esta en la etapa '{solicitud.Etapa}', no se puede procesar en el paso 3.");
        }

        if (solicitud.AnalistaId != analistaId)
        {
            throw new ForbiddenException("workflow.analista.no_asignado", "No eres el analista asignado a esta solicitud.");
        }

        return solicitud;
    }

    private async Task<Solicitud> ObtenerSolicitudEnEtapasAsync(
        int id,
        IReadOnlyCollection<EtapaSolicitud> etapas,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        if (!etapas.Contains(solicitud.Etapa))
        {
            throw new DomainValidationException(code, message);
        }

        return solicitud;
    }

    private async Task<Solicitud> ObtenerSolicitudAsignadaAsync(
        int id,
        int actorId,
        Func<Solicitud, int?> asignacionSelector,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        ValidarActorAsignado(asignacionSelector(solicitud), actorId, code, message);

        return solicitud;
    }

    private async Task EjecutarEnvioProveedorAsync(Solicitud solicitud, CancellationToken cancellationToken)
    {
        var proveedor = solicitud.Proveedores.FirstOrDefault(proveedor => proveedor.Seleccionado)
            ?? throw new DomainValidationException(
                "workflow.envio_proveedor.proveedor_requerido",
                "No hay un proveedor seleccionado para enviar la orden de compra.");

        if (solicitud.AnalistaId is null)
        {
            var analista = await asignacionUsuarioService.AsignarSiguienteAsync(RolUsuario.Analista, cancellationToken);
            solicitud.AsignarAnalista(analista.Id);
        }

        solicitud.CambiarEtapa(EtapaSolicitud.RevisionAnomalias, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            $"Orden de compra enviada al proveedor {proveedor.Nombre}",
            actorId: null));
    }

    private static void ValidarActorAsignado(int? asignadoId, int actorId, string code, string message)
    {
        if (asignadoId != actorId)
        {
            throw new ForbiddenException(code, message);
        }
    }

    private static void RequerirComentario(string? comentario, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(comentario))
        {
            throw new DomainValidationException(code, message);
        }
    }

    private static TipoDocumento ObtenerTipoDocumento(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new DomainValidationException("documento.tipo.requerido", "El tipo de documento es obligatorio.");
        }

        return tipo.Trim().ToLowerInvariant() switch
        {
            "cotizacion" => TipoDocumento.Cotizacion,
            "certificado_existencia" => TipoDocumento.CertificadoExistencia,
            "rut" => TipoDocumento.Rut,
            "orden_compra" => TipoDocumento.OrdenCompra,
            "factura" => TipoDocumento.Factura,
            "soporte_contable" => TipoDocumento.SoporteContable,
            _ => throw new DomainValidationException("documento.tipo.invalido", "Tipo de documento invalido.")
        };
    }

    private async Task<SolicitudDetalleResponse> ObtenerDetalleActualizadoAsync(int id, CancellationToken cancellationToken)
    {
        var detalle = await solicitudRepository.GetDetailByIdAsync(id, cancellationToken);

        return SolicitudMapper.ToDetalle(detalle!);
    }
}