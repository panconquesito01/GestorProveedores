using System.Security.Claims;
using GestorProveedores.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestorProveedores.WebApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int UsuarioId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)
        ? usuarioId
        : throw new UnauthorizedAccessException("Usuario no valido.");

    protected bool TryMapKnownException(Exception exception, out ActionResult result)
    {
        result = exception switch
        {
            UnauthorizedAccessException => Unauthorized(ToProblemDetails(StatusCodes.Status401Unauthorized, "Usuario no valido")),
            ForbiddenException forbidden => StatusCode(StatusCodes.Status403Forbidden, ToProblemDetails(StatusCodes.Status403Forbidden, forbidden.Message, forbidden.Code)),
            NotFoundException notFound => NotFound(ToProblemDetails(StatusCodes.Status404NotFound, notFound.Message, notFound.Code)),
            DomainValidationException validation => BadRequest(ToProblemDetails(StatusCodes.Status400BadRequest, validation.Message, validation.Code)),
            _ => null!
        };

        return result is not null;
    }

    protected static ProblemDetails ToProblemDetails(int statusCode, string title, string? code = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title
        };

        if (code is not null)
        {
            problemDetails.Extensions["code"] = code;
        }

        return problemDetails;
    }
}