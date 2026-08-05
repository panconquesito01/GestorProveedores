using GestorProveedores.Business.Ports;
using GestorProveedores.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GestorProveedores.Infrastructure.Authentication;

internal sealed class AspNetPasswordHashGenerator : IPasswordHashGenerator
{
    private readonly PasswordHasher<Usuario> passwordHasher = new();

    public string Generate(string password) => passwordHasher.HashPassword(null!, password);
}