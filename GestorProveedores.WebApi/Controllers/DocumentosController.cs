using GestorProveedores.Business.Documentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorProveedores.WebApi.Controllers;

[ApiController]
[Route("api/documentos")]
[Authorize]
public sealed class DocumentosController(IDocumentoService documentoService) : ApiControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Descargar(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await documentoService.DescargarAsync(id, UsuarioId, cancellationToken);
            Response.Headers.ContentDisposition = $"inline; filename=\"{response.NombreArchivo}\"";

            return File(response.Contenido, response.MimeType);
        }
        catch (Exception exception) when (TryMapKnownException(exception, out var result))
        {
            return result;
        }
    }
}