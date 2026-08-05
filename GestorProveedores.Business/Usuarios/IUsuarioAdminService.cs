namespace GestorProveedores.Business.Usuarios;

public interface IUsuarioAdminService
{
    Task<IReadOnlyList<UsuarioResponse>> ListarAsync(int? actorId, CancellationToken cancellationToken = default);

    Task<UsuarioResponse> CrearAsync(UsuarioCreateRequest request, int? actorId, CancellationToken cancellationToken = default);
}