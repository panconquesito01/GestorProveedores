using GestorProveedores.Business.Authentication;

namespace GestorProveedores.WebApp.Services;

public sealed class AppSessionState
{
    public LoginResponse? CurrentSession { get; private set; }

    public bool IsAuthenticated => CurrentSession is not null;

    public void SignIn(LoginResponse session) => CurrentSession = session;

    public void SignOut() => CurrentSession = null;
}