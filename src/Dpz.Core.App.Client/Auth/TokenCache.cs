namespace Dpz.Core.App.Client.Auth;

public sealed class TokenCache
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public DateTimeOffset? RefreshTokenExpiresAtUtc { get; init; }

    public string TokenType { get; init; } = "Bearer";

    public string Scope { get; init; } = string.Empty;

    public bool IsAccessTokenValid(DateTimeOffset nowUtc, TimeSpan skew)
    {
        if (string.IsNullOrWhiteSpace(AccessToken))
        {
            return false;
        }

        return ExpiresAtUtc > nowUtc.Add(skew);
    }

    public bool CanRefresh(DateTimeOffset nowUtc, TimeSpan skew)
    {
        if (string.IsNullOrWhiteSpace(RefreshToken))
        {
            return false;
        }

        if (!RefreshTokenExpiresAtUtc.HasValue)
        {
            return true;
        }

        return RefreshTokenExpiresAtUtc.Value > nowUtc.Add(skew);
    }
}
