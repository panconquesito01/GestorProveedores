namespace GestorProveedores.Business.Catalogos;

public interface ICatalogoService
{
    Task<IReadOnlyList<EmpresaResponse>> ListarEmpresasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AprobadorOptionResponse>> ListarAprobadoresAsync(int empresaId, CancellationToken cancellationToken = default);
}