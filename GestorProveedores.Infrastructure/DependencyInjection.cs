using GestorProveedores.Business.Ports;
using GestorProveedores.Business.Authentication;
using GestorProveedores.Infrastructure.Authentication;
using GestorProveedores.Infrastructure.Persistence;
using GestorProveedores.Infrastructure.Repositories;
using GestorProveedores.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestorProveedores.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        }

        services.AddDbContext<GestorProveedoresDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        services.AddScoped<IEmpresaReadRepository, EmpresaReadRepository>();
        services.AddScoped<IUsuarioReadRepository, UsuarioReadRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISolicitudRepository, SolicitudRepository>();
        services.AddScoped<IAsignacionUsuarioService, RoundRobinAsignacionUsuarioService>();
        services.AddScoped<IRadicadoGenerator, SqlServerRadicadoGenerator>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<GestorProveedoresDbContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashGenerator, AspNetPasswordHashGenerator>();
        services.AddScoped<IPasswordHashVerifier, AspNetPasswordHashVerifier>();

        return services;
    }
}