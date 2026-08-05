using GestorProveedores.Business;
using GestorProveedores.Business.Authentication;
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static bool IsLocalReturnUrl(string? returnUrl) =>
    !string.IsNullOrWhiteSpace(returnUrl)
    && returnUrl.StartsWith('/')
    && !returnUrl.StartsWith("//")
    && !returnUrl.Contains("://", StringComparison.Ordinal);
