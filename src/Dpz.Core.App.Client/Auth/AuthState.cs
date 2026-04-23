namespace Dpz.Core.App.Client.Auth;

public enum AuthState
{
    Unauthenticated = 0,
    Authenticating = 1,
    Authenticated = 2,
    Refreshing = 3,
    ReauthenticationRequired = 4,
}
