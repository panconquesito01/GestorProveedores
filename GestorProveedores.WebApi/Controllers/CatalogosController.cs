using GestorProveedores.Business.Authentication;
using GestorProveedores.Business.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorProveedores.WebApi.Controllers;

[ApiController]
[Route("api/catalogos")]
[Produces("application/json")]
[Authorize]
public sealed class CatalogosController(ICatalogoService catalogoService) : ApiControllerBase
{
    [HttpGet("empresas")]
    [ProducesResponseType(typeof(IReadOnlyList<EmpresaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<EmpresaResponse>>> ListarEmpresas(
        CancellationToken cancellationToken)
    {
        var response = await catalogoService.ListarEmpresasAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("aprobadores")]
    [ProducesResponseType(typeof(IReadOnlyList<AprobadorOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AprobadorOptionResponse>>> ListarAprobadores(
        [FromQuery(Name = "empresa_id")] int empresaId,
        CancellationToken cancellationToken)
    {
        if (empresaId <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "empresa_id es requerido y debe ser mayor que cero"
            });
        }

        var response = await catalogoService.ListarAprobadoresAsync(empresaId, cancellationToken);

        return Ok(response);
    }
}