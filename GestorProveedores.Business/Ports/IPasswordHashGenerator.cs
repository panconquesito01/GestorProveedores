namespace GestorProveedores.Business.Ports;

public interface IPasswordHashGenerator
{
    string Generate(string password);
}