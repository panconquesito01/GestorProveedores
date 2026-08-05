using GestorProveedores.Domain.Entities;

namespace GestorProveedores.Business.Ports;

public interface IPasswordHashVerifier
{
    bool Verify(Usuario usuario, string password);
}