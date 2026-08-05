namespace GestorProveedores.WebApp.Services;

public sealed record SolicitudViewDefinition(
    string Name,
    string Label,
    string Description,
    bool IsOverview = false);