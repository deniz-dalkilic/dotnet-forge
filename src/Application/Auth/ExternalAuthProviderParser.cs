using Template.Domain.Entities;

namespace Template.Application.Auth;

public static class ExternalAuthProviderParser
{
    public static bool TryParse(string input, out ExternalAuthProvider provider)
    {
        provider = default;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        switch (input.Trim().ToLowerInvariant())
        {
            case "google":
                provider = ExternalAuthProvider.Google;
                return true;
            case "microsoft":
            case "ms":
                provider = ExternalAuthProvider.Microsoft;
                return true;
            case "apple":
                provider = ExternalAuthProvider.Apple;
                return true;
            default:
                return false;
        }
    }
}
