using GestorProveedores.Business.Solicitudes;
using GestorProveedores.Business.Workflow;
using GestorProveedores.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GestorProveedores.WebApi.Controllers;

[ApiController]
[Route("api/workflow")]
[Produces("application/json")]
[Authorize]
public sealed class WorkflowController(IWorkflowService workflowService) : ApiControllerBase
{
    [HttpPost("{id:int}/paso1/devolver")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> DevolverPaso1(
        int id,
        [FromBody] ComentarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.DevolverPaso1Async(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso1/siguiente")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> AvanzarPaso1(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.AvanzarPaso1Async(id, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso2/proveedor-erp")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> RegistrarProveedorErp(
        int id,
        [FromBody] ProveedorErpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.RegistrarProveedorErpAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso2/proveedor-erp/siguiente")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> AvanzarProveedorErp(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.AvanzarProveedorErpAsync(id, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso2/proveedores-nuevos")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> GuardarProveedoresNuevos(
        int id,
        [FromBody] ProveedoresNuevosRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.GuardarProveedoresNuevosAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso2/proveedores/{proveedorId:int}/creado-en-erp")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> MarcarProveedorCreadoEnErp(
        int id,
        int proveedorId,
        [FromBody] CreadoEnErpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.MarcarProveedorCreadoEnErpAsync(id, proveedorId, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso2/proveedores-nuevos/siguiente")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> AvanzarProveedoresNuevos(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.AvanzarProveedoresNuevosAsync(id, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso3/seleccionar")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> SeleccionarProveedor(
        int id,
        [FromBody] SeleccionarProveedorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.SeleccionarProveedorAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/proveedores/{proveedorId:int}/documento")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> SubirDocumentoProveedor(
        int id,
        int proveedorId,
        [FromForm] string tipo,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        try
        {
            var archivo = await ToArchivoAsync(file, "documento", cancellationToken);
            var response = await workflowService.SubirDocumentoProveedorAsync(id, proveedorId, tipo, archivo, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso4/orden-compra")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> CargarOrdenCompra(
        int id,
        [FromForm] IFormFile? file,
        [FromForm] string? comentario,
        CancellationToken cancellationToken)
    {
        try
        {
            var archivo = await ToArchivoAsync(file, "orden_compra", cancellationToken);
            var response = await workflowService.CargarOrdenCompraAsync(id, archivo, comentario, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso5/solicitante")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> RevisarOrdenCompraSolicitante(
        int id,
        [FromBody] DecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.RevisarOrdenCompraSolicitanteAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso5/aprobador")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> RevisarOrdenCompraAprobador(
        int id,
        [FromBody] DecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.RevisarOrdenCompraAprobadorAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso6/factura")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> CargarFactura(
        int id,
        [FromForm] IFormFile? file,
        [FromForm] string? comentario,
        CancellationToken cancellationToken)
    {
        try
        {
            var archivo = await ToArchivoAsync(file, "factura", cancellationToken);
            var response = await workflowService.CargarFacturaAsync(id, archivo, comentario, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso7/solicitante")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> RevisarFacturaSolicitante(
        int id,
        [FromBody] DecisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.RevisarFacturaSolicitanteAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso8/contabilidad")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> EnviarContabilidad(
        int id,
        [FromForm] List<IFormFile>? files,
        [FromForm] string? comentario,
        CancellationToken cancellationToken)
    {
        try
        {
            var soportes = new List<ArchivoWorkflowRequest>();
            foreach (var file in files ?? [])
            {
                soportes.Add(await ToArchivoAsync(file, "soporte", cancellationToken));
            }

            var response = await workflowService.EnviarContabilidadAsync(id, soportes, comentario, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso9/conforme")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> MarcarConformeContabilidad(
        int id,
        [FromBody] ConformeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.MarcarConformeContabilidadAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso9/objetar")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> ObjetarContabilidad(
        int id,
        [FromBody] ObjetarContableRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await workflowService.ObjetarContabilidadAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost("{id:int}/paso9/reenviar-factura")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> ReenviarFacturaObjetada(
        int id,
        [FromForm] IFormFile? file,
        [FromForm] string? comentario,
        CancellationToken cancellationToken)
    {
        try
        {
            var archivo = await ToArchivoAsync(file, "factura", cancellationToken);
            var response = await workflowService.ReenviarFacturaObjetadaAsync(id, archivo, comentario, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    private static async Task<ArchivoWorkflowRequest> ToArchivoAsync(
        IFormFile? file,
        string fallbackName,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new DomainValidationException("documento.archivo.requerido", "El archivo es obligatorio.");
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        return new ArchivoWorkflowRequest(
            string.IsNullOrWhiteSpace(file.FileName) ? fallbackName : file.FileName,
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            stream.ToArray());
    }

}