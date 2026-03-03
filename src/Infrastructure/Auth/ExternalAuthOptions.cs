using System.ComponentModel.DataAnnotations;

namespace Template.Infrastructure.Auth;

public sealed class ExternalAuthOptions
{
    public const string SectionName = "ExternalAuth";

    [Required]
    public ProviderOptions Providers { get; init; } = new();

    public string GoogleDiscoveryUrl { get; init; } = "https://accounts.google.com/.well-known/openid-configuration";
    public string MicrosoftDiscoveryUrlTemplate { get; init; } = "https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration";
    public string AppleDiscoveryUrl { get; init; } = "https://appleid.apple.com/.well-known/openid-configuration";

    public sealed class ProviderOptions
    {
        [Required]
        public GoogleProviderOptions Google { get; init; } = new();

        [Required]
        public MicrosoftProviderOptions Microsoft { get; init; } = new();

        [Required]
        public AppleProviderOptions Apple { get; init; } = new();
    }

    public sealed class GoogleProviderOptions
    {
        [Required]
        public string ClientId { get; init; } = string.Empty;
    }

    public sealed class MicrosoftProviderOptions
    {
        [Required]
        public string ClientId { get; init; } = string.Empty;

        public string Tenant { get; init; } = "common";
    }

    public sealed class AppleProviderOptions
    {
        [Required]
        public string ClientId { get; init; } = string.Empty;
    }
}
