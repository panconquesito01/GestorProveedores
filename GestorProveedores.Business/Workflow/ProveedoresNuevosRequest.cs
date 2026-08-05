namespace GestorProveedores.Business.Workflow;

public sealed record ProveedoresNuevosRequest(IReadOnlyList<ProveedorNuevoItem> Candidatos);