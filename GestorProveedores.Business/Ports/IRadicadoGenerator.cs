namespace GestorProveedores.Business.Ports;

public interface IRadicadoGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}