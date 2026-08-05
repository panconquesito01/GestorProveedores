using GestorProveedores.Business;
using GestorProveedores.Business.Authentication;
using GestorProveedores.Business.Solicitudes;
using GestorProveedores.Business.Usuarios;
using GestorProveedores.Infrastructure;
using GestorProveedores.WebApp.Components;
using GestorProveedores.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBusinessServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<WebAppSessionService>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapPost("/account/login", async (HttpContext context, IAuthService authService, WebAppSessionService sessionService) =>
{
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var identificador = form["identificador"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var session = await authService.LoginAsync(new LoginRequest(identificador, password), context.RequestAborted);
    if (session is null)
    {
        return Results.Redirect("/login?error=1");
    }

    sessionService.SignIn(context, session);

    return Results.Redirect(IsLocalReturnUrl(returnUrl) ? returnUrl : "/");
});

app.MapPost("/account/logout", (HttpContext context, WebAppSessionService sessionService) =>
{
    sessionService.SignOut(context);
    return Results.Redirect("/login");
});

app.MapPost("/solicitudes/crear", async (HttpContext context, WebAppSessionService sessionService, ISolicitudService solicitudService) =>
{
    var session = sessionService.GetCurrentSession();
    if (session is null)
    {
        return Results.Redirect("/login?returnUrl=/solicitudes/nueva");
    }

    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var aprobador = form["aprobadorId"].ToString();
    var request = new SolicitudCreateRequest(
        form["titulo"].ToString(),
        form["descripcion"].ToString(),
        ToOptionalString(form["frecuencia"].ToString()),
        int.TryParse(aprobador, out var aprobadorId) ? aprobadorId : null);

    try
    {
        var detalle = await solicitudService.CrearAsync(request, session.Usuario.Id, context.RequestAborted);
        return Results.Redirect($"/solicitudes/{detalle.Id}?creada=1");
    }
    catch (Exception exception)
    {
        return Results.Redirect(BuildNuevaSolicitudErrorUrl(form, exception.Message));
    }
});

app.MapPost("/superusuario/usuarios/crear", async (HttpContext context, WebAppSessionService sessionService, IUsuarioAdminService usuarioAdminService) =>
{
    var session = sessionService.GetCurrentSession();
    if (session is null)
    {
        return Results.Redirect("/login?returnUrl=/superusuario");
    }

    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var empresa = form["empresaId"].ToString();
    var request = new UsuarioCreateRequest(
        form["nombre"].ToString(),
        form["email"].ToString(),
        form["username"].ToString(),
        form["password"].ToString(),
        form["rol"].ToString(),
        int.TryParse(empresa, out var empresaId) ? empresaId : null);

    try
    {
        var usuario = await usuarioAdminService.CrearAsync(request, session.Usuario.Id, context.RequestAborted);
        return Results.Redirect($"/superusuario?creado={usuario.Id}");
    }
    catch (Exception exception)
    {
        return Results.Redirect(BuildSuperUsuarioErrorUrl(form, exception.Message));
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static bool IsLocalReturnUrl(string? returnUrl) =>
    !string.IsNullOrWhiteSpace(returnUrl)
    && returnUrl.StartsWith('/')
    && !returnUrl.StartsWith("//")
    && !returnUrl.Contains("://", StringComparison.Ordinal);

static string? ToOptionalString(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

static string BuildNuevaSolicitudErrorUrl(IFormCollection form, string message)
{
    var parameters = new List<string>
    {
        $"error={Uri.EscapeDataString(message)}"
    };

    AddQueryParameter(parameters, "titulo", form["titulo"].ToString());
    AddQueryParameter(parameters, "descripcion", form["descripcion"].ToString());
    AddQueryParameter(parameters, "frecuencia", form["frecuencia"].ToString());
    AddQueryParameter(parameters, "aprobadorId", form["aprobadorId"].ToString());

    return "/solicitudes/nueva?" + string.Join('&', parameters);
}

static void AddQueryParameter(List<string> parameters, string name, string value)
{
    if (!string.IsNullOrWhiteSpace(value))
    {
        parameters.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
    }
}

static string BuildSuperUsuarioErrorUrl(IFormCollection form, string message)
{
    var parameters = new List<string>
    {
        $"error={Uri.EscapeDataString(message)}"
    };

    AddQueryParameter(parameters, "nombre", form["nombre"].ToString());
    AddQueryParameter(parameters, "email", form["email"].ToString());
    AddQueryParameter(parameters, "username", form["username"].ToString());
    AddQueryParameter(parameters, "rol", form["rol"].ToString());
    AddQueryParameter(parameters, "empresaId", form["empresaId"].ToString());

    return "/superusuario?" + string.Join('&', parameters);
}
