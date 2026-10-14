using Microsoft.AspNetCore.Authentication;

namespace DotnetRagApi.Api.Security;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "Security";

    public string ApiKey { get; set; } = string.Empty;
}
