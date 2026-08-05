namespace GestorProveedores.Business.Workflow;

public sealed record DecisionRequest(bool Aprobado, string? Comentario);