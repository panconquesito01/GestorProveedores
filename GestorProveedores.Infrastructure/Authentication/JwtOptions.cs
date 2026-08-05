namespace GestorProveedores.Infrastructure.Authentication;

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "GestorProveedores";
    public string Audience { get; init; } = "GestorProveedores.WebApi";
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}