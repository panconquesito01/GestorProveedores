using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestorProveedores.Business.Authentication;
using GestorProveedores.Domain.Entities;
using GestorProveedores.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GestorProveedores.Infrastructure.Authentication;

internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions options;

    public JwtTokenService(IConfiguration configuration)
    {
        options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(options.SigningKey) || Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured with at least 32 bytes.");
        }
    }

    public JwtTokenResult CreateToken(Usuario usuario)
    {
        var now = DateTime.UtcNow;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, ToApiRole(usuario.Rol)),
            new("name", usuario.Nombre)
        };

        if (usuario.EmpresaId is not null)
        {
            claims.Add(new Claim("empresa_id", usuario.EmpresaId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            notBefore: now,
            expires: now.AddMinutes(options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            checked(options.ExpirationMinutes * 60));
    }

    private static string ToApiRole(RolUsuario rol) => rol switch
    {
        RolUsuario.Solicitante => "solicitante",
        RolUsuario.Auxiliar => "auxiliar",
        RolUsuario.Analista => "analista",
        RolUsuario.Aprobador => "aprobador",
        RolUsuario.Contable => "contable",
        _ => throw new InvalidOperationException($"Rol de usuario no soportado: {rol}.")
    };
}