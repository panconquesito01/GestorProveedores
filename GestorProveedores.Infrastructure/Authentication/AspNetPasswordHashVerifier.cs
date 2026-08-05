using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GestorProveedores.Infrastructure.Authentication;

internal sealed class AspNetPasswordHashVerifier : IPasswordHashVerifier
{
    private readonly PasswordHasher<Usuario> passwordHasher = new();

    public bool Verify(Usuario usuario, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var result = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}