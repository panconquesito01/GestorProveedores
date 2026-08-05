using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using GestorProveedores.Domain.Exceptions;

namespace GestorProveedores.Business.Solicitudes;

internal sealed class SolicitudService(
    IUsuarioReadRepository usuarioReadRepository,
    ISolicitudRepository solicitudRepository,
    IAsignacionUsuarioService asignacionUsuarioService,
    IRadicadoGenerator radicadoGenerator,
    IUnitOfWork unitOfWork) : ISolicitudService
{
    private static readonly IReadOnlyDictionary<string, VistaSolicitud> Vistas = new Dictionary<string, VistaSolicitud>(StringComparer.OrdinalIgnoreCase)
    {
        ["solicitante_mias"] = new([RolUsuario.Solicitante], null, SolicitudAsignacionCampo.Solicitante),
        ["solicitante_oc_revisar"] = new([RolUsuario.Solicitante], [EtapaSolicitud.RevisionOcSolicitante], SolicitudAsignacionCampo.Solicitante),
        ["solicitante_facturas_revisar"] = new([RolUsuario.Solicitante], [EtapaSolicitud.RevisionFacturaSolicitante], SolicitudAsignacionCampo.Solicitante),
        ["auxiliar_paso1"] = new([RolUsuario.Auxiliar], [EtapaSolicitud.RevisionAuxiliar], SolicitudAsignacionCampo.Auxiliar),
        ["auxiliar_paso2"] = new([RolUsuario.Auxiliar], [EtapaSolicitud.RevisionProveedores], SolicitudAsignacionCampo.Auxiliar),
        ["auxiliar_paso4"] = new([RolUsuario.Auxiliar], [EtapaSolicitud.CargaOrdenCompra], SolicitudAsignacionCampo.Auxiliar),
        ["auxiliar_oc_devueltas"] = new([RolUsuario.Auxiliar], [EtapaSolicitud.OcDevueltaAuxiliar], SolicitudAsignacionCampo.Auxiliar),
        ["auxiliar_facturas_aprobadas"] = new([RolUsuario.Auxiliar], [EtapaSolicitud.FacturaAprobadaAuxiliar], SolicitudAsignacionCampo.Auxiliar),
        ["auxiliar_facturas_objetadas"] = new([RolUsuario.Auxiliar], [EtapaSolicitud.FacturaObjetadaContable], SolicitudAsignacionCampo.Auxiliar),
        ["analista_seleccion_proveedor"] = new([RolUsuario.Analista], [EtapaSolicitud.SeleccionProveedor], SolicitudAsignacionCampo.Analista),
        ["analista_revision_anomalias"] = new([RolUsuario.Analista], [EtapaSolicitud.RevisionAnomalias, EtapaSolicitud.FacturaDevueltaAnalista], SolicitudAsignacionCampo.Analista),
        ["aprobador_pendientes"] = new([RolUsuario.Aprobador], [EtapaSolicitud.RevisionOcAprobador], SolicitudAsignacionCampo.Aprobador),
        ["contable_facturas_validar"] = new([RolUsuario.Contable], [EtapaSolicitud.ValidacionContable], SolicitudAsignacionCampo.Ninguno)
    };

    public async Task<IReadOnlyList<SolicitudListItemResponse>> ListarAsync(
        SolicitudListQuery query,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerUsuarioActualAsync(usuarioId, cancellationToken);

        if (string.IsNullOrWhiteSpace(query.Vista) || !Vistas.TryGetValue(query.Vista.Trim(), out var vista))
        {
            throw new DomainValidationException("solicitudes.vista.invalida", "Vista invalida.");
        }

        if (!vista.Roles.Contains(usuario.Rol))
        {
            throw new ForbiddenException("solicitudes.vista.sin_permiso", "No tienes acceso a esta vista.");
        }

        var criteria = new SolicitudListCriteria(
            vista.Etapas,
            vista.AsignacionCampo,
            usuario.Id,
            query.SolicitanteNombre,
            query.EmpresaId,
            query.FechaDesde,
            query.FechaHasta);

        var solicitudes = await solicitudRepository.ListAsync(criteria, cancellationToken);

        return solicitudes.Select(SolicitudMapper.ToListItem).ToList();
    }

    public async Task<SolicitudDetalleResponse> CrearAsync(
        SolicitudCreateRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var solicitante = await ObtenerUsuarioActualAsync(usuarioId, cancellationToken);

        if (solicitante.Rol is not RolUsuario.Solicitante)
        {
            throw new ForbiddenException("solicitudes.crear.rol_invalido", "Solo el rol solicitante puede crear solicitudes.");
        }

        if (solicitante.EmpresaId is null)
        {
            throw new DomainValidationException("solicitudes.crear.empresa_requerida", "El solicitante debe tener empresa asociada.");
        }

        if (request.AprobadorId is not null)
        {
            await ValidarAprobadorAsync(request.AprobadorId.Value, solicitante.EmpresaId.Value, cancellationToken);
        }

        var auxiliar = await asignacionUsuarioService.AsignarSiguienteAsync(RolUsuario.Auxiliar, cancellationToken);
        var radicado = await radicadoGenerator.GenerateAsync(cancellationToken);

        var solicitud = Solicitud.Crear(
            radicado,
            request.Titulo,
            request.Descripcion,
            request.Frecuencia,
            solicitante.Id,
            solicitante.EmpresaId.Value,
            request.AprobadorId,
            auxiliar.Id);

        solicitudRepository.Add(solicitud);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Solicitud radicada",
            solicitante.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var detalle = await solicitudRepository.GetDetailByIdAsync(solicitud.Id, cancellationToken);

        return SolicitudMapper.ToDetalle(detalle!);
    }

    public async Task<SolicitudDetalleResponse> EditarYReenviarAsync(
        int id,
        SolicitudUpdateRequest request,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var solicitante = await ObtenerUsuarioActualAsync(usuarioId, cancellationToken);

        if (solicitante.Rol is not RolUsuario.Solicitante)
        {
            throw new ForbiddenException("solicitudes.editar.rol_invalido", "Solo el rol solicitante puede editar solicitudes devueltas.");
        }

        var solicitud = await solicitudRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        if (solicitud.Etapa is not EtapaSolicitud.DevueltaSolicitante)
        {
            throw new DomainValidationException(
                "solicitudes.editar.etapa_invalida",
                $"La solicitud esta en la etapa '{solicitud.Etapa}', no se puede editar y reenviar.");
        }

        if (solicitud.SolicitanteId != solicitante.Id)
        {
            throw new ForbiddenException("solicitudes.editar.solicitante_invalido", "No eres el solicitante asignado a esta solicitud.");
        }

        if (solicitante.EmpresaId is null)
        {
            throw new DomainValidationException("solicitudes.editar.empresa_requerida", "El solicitante debe tener empresa asociada.");
        }

        if (request.AprobadorId is not null)
        {
            await ValidarAprobadorAsync(request.AprobadorId.Value, solicitante.EmpresaId.Value, cancellationToken);
        }

        solicitud.ActualizarDatosBasicos(request.Titulo, request.Descripcion, request.Frecuencia, request.AprobadorId);
        solicitud.CambiarEtapa(EtapaSolicitud.RevisionAuxiliar, EstadoSolicitud.EnProceso);

        solicitudRepository.AddHistorial(SolicitudHistorial.Crear(
            solicitud.Id,
            solicitud.Etapa,
            "Solicitud editada y reenviada",
            solicitante.Id));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var detalle = await solicitudRepository.GetDetailByIdAsync(solicitud.Id, cancellationToken);

        return SolicitudMapper.ToDetalle(detalle!);
    }

    public async Task<SolicitudDetalleResponse> ObtenerDetalleAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        _ = await ObtenerUsuarioActualAsync(usuarioId, cancellationToken);

        var solicitud = await solicitudRepository.GetDetailByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("solicitudes.no_encontrada", "Solicitud no encontrada.");

        return SolicitudMapper.ToDetalle(solicitud);
    }

    private async Task<Usuario> ObtenerUsuarioActualAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        if (usuarioId is null or <= 0)
        {
            throw new UnauthorizedAccessException("Usuario no valido.");
        }

        return await usuarioReadRepository.GetActiveByIdAsync(usuarioId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuario no valido.");
    }

    private async Task ValidarAprobadorAsync(int aprobadorId, int empresaId, CancellationToken cancellationToken)
    {
        var aprobador = await usuarioReadRepository.GetActiveByIdAsync(aprobadorId, cancellationToken);

        if (aprobador is null || aprobador.Rol is not RolUsuario.Aprobador || aprobador.EmpresaId != empresaId)
        {
            throw new DomainValidationException(
                "solicitudes.aprobador.invalido",
                "El aprobador seleccionado no existe, no esta activo o no pertenece a la empresa del solicitante.");
        }
    }

    private sealed record VistaSolicitud(
        IReadOnlyCollection<RolUsuario> Roles,
        IReadOnlyCollection<EtapaSolicitud>? Etapas,
        SolicitudAsignacionCampo AsignacionCampo);
}