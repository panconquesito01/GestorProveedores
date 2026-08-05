using System.Security.Claims;
using GestorProveedores.Business.Ports;
using Microsoft.AspNetCore.Authorization;

namespace GestorProveedores.WebApi.Authorization;

internal sealed class ActiveUserAuthorizationHandler(IUsuarioReadRepository usuarioReadRepository)
    : AuthorizationHandler<ActiveUserRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        var usuarioIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdValue, out var usuarioId))
        {
            return;
        }

        var usuario = await usuarioReadRepository.GetActiveByIdAsync(usuarioId);
        if (usuario is not null)
        {
            context.Succeed(requirement);
        }
    }
}