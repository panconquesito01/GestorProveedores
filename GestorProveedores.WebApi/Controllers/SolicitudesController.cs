using GestorProveedores.Business.Solicitudes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorProveedores.WebApi.Controllers;

[ApiController]
[Route("api/solicitudes")]
[Produces("application/json")]
[Authorize]
public sealed class SolicitudesController(ISolicitudService solicitudService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SolicitudListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<SolicitudListItemResponse>>> Listar(
        [FromQuery] string vista,
        [FromQuery(Name = "solicitante_nombre")] string? solicitanteNombre,
        [FromQuery(Name = "empresa_id")] int? empresaId,
        [FromQuery(Name = "fecha_desde")] DateOnly? fechaDesde,
        [FromQuery(Name = "fecha_hasta")] DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new SolicitudListQuery(vista, solicitanteNombre, empresaId, fechaDesde, fechaHasta);
            var response = await solicitudService.ListarAsync(query, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SolicitudDetalleResponse>> Crear(
        [FromBody] SolicitudCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await solicitudService.CrearAsync(request, UsuarioId, cancellationToken);

            return CreatedAtAction(nameof(Obtener), new { id = response.Id }, response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> Editar(
        int id,
        [FromBody] SolicitudUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await solicitudService.EditarYReenviarAsync(id, request, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SolicitudDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalleResponse>> Obtener(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await solicitudService.ObtenerDetalleAsync(id, UsuarioId, cancellationToken);

            return Ok(response);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }

}