using GestorProveedores.Business.Ports;

namespace GestorProveedores.Business.Catalogos;

internal sealed class CatalogoService(
    IEmpresaReadRepository empresaReadRepository,
    IUsuarioReadRepository usuarioReadRepository) : ICatalogoService
{
    public async Task<IReadOnlyList<EmpresaResponse>> ListarEmpresasAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await empresaReadRepository.ListAsync(cancellationToken);

        return empresas
            .Select(empresa => new EmpresaResponse(empresa.Id, empresa.Nombre, empresa.Nit))
            .ToList();
    }

    public async Task<IReadOnlyList<AprobadorOptionResponse>> ListarAprobadoresAsync(int empresaId, CancellationToken cancellationToken = default)
    {
        var aprobadores = await usuarioReadRepository.ListActiveApproversByEmpresaAsync(empresaId, cancellationToken);
        var opciones = new List<AprobadorOptionResponse>
        {
            new(null, "No requiere aprobacion")
        };

        opciones.AddRange(aprobadores.Select(aprobador => new AprobadorOptionResponse(aprobador.Id, aprobador.Nombre)));

        return opciones;
    }
}