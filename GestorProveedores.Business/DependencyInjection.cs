using GestorProveedores.Business.Authentication;
using GestorProveedores.Business.Catalogos;
using GestorProveedores.Business.Documentos;
using GestorProveedores.Business.Solicitudes;
using GestorProveedores.Business.Usuarios;
using GestorProveedores.Business.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace GestorProveedores.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioActualService, UsuarioActualService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IDocumentoService, DocumentoService>();
        services.AddScoped<ISolicitudService, SolicitudService>();
        services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();
        services.AddScoped<IWorkflowService, WorkflowService>();

        return services;
    }
}