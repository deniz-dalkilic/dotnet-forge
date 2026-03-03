using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions;
using Template.Application.Auth;
using Template.Infrastructure.Auth;

namespace Template.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/external/{provider}", async (
            string provider,
            ExternalSignInRequest request,
            ExternalSignInService signInService,
            IJwtTokenIssuer tokenIssuer,
            IOptions<JwtOptions> jwtOptions,
            CancellationToken cancellationToken) =>
        {
            if (!ExternalAuthProviderParser.TryParse(provider, out var parsedProvider))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid external auth provider.",
                    detail: "Supported providers are google, microsoft, and apple.");
            }

            try
            {
                var signInResult = await signInService.SignInAsync(parsedProvider, request.IdToken, request.Nonce, cancellationToken);
                var lifetime = TimeSpan.FromMinutes(jwtOptions.Value.LifetimeMinutes);
                var accessToken = tokenIssuer.IssueAccessToken(
                    signInResult.User,
                    signInResult.Roles,
                    lifetime,
                    new Dictionary<string, string>
                    {
                        ["idp"] = ToProviderClaim(parsedProvider)
                    });

                return Results.Ok(new ExternalSignInResponse(
                    accessToken,
                    (int)lifetime.TotalSeconds,
                    new UserResponse(signInResult.User.Id, signInResult.User.Email, signInResult.User.DisplayName),
                    signInResult.Roles));
            }
            catch (ExternalAuthValidationException ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid external token.",
                    detail: ex.Message);
            }
        })
        .WithTags("Auth")
        .WithSummary("Exchanges an external ID token for an internal access token.");

        endpoints.MapGet("/api/me", (ClaimsPrincipal user) =>
        {
            var claims = user.Claims
                .Select(claim => new ClaimSummary(claim.Type, claim.Value))
                .ToArray();

            return Results.Ok(new MeResponse(
                user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"),
                user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email"),
                user.Identity?.Name ?? user.FindFirstValue("name"),
                user.FindAll("roles").Select(x => x.Value).ToArray(),
                claims));
        })
        .RequireAuthorization()
        .WithTags("Auth")
        .WithSummary("Returns current user claim summary.");

        return endpoints;
    }

    public sealed record ExternalSignInRequest(string IdToken, string? Nonce);

    public sealed record ExternalSignInResponse(
        string AccessToken,
        int ExpiresIn,
        UserResponse User,
        IReadOnlyList<string> Roles);

    public sealed record UserResponse(Guid Id, string? Email, string DisplayName);

    public sealed record MeResponse(
        string? Subject,
        string? Email,
        string? Name,
        IReadOnlyList<string> Roles,
        IReadOnlyList<ClaimSummary> Claims);

    public sealed record ClaimSummary(string Type, string Value);

    private static string ToProviderClaim(Template.Domain.Entities.ExternalAuthProvider provider)
    {
        return provider switch
        {
            Template.Domain.Entities.ExternalAuthProvider.Google => "google",
            Template.Domain.Entities.ExternalAuthProvider.Microsoft => "microsoft",
            Template.Domain.Entities.ExternalAuthProvider.Apple => "apple",
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
}
