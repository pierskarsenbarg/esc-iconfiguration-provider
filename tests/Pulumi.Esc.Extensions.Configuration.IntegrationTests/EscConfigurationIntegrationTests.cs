using Microsoft.Extensions.Configuration;

namespace Pulumi.Esc.Extensions.Configuration.IntegrationTests;

/// <summary>
/// Integration tests that exercise the real ESC SDK against a live environment.
///
/// Required environment variables:
///   ESC_TEST_ORG         - Pulumi organisation name
///   ESC_TEST_PROJECT     - ESC project name
///   ESC_TEST_ENVIRONMENT - ESC environment name
///
/// For tests that pass an explicit token:
///   ESC_TEST_ACCESS_TOKEN - Pulumi access token passed directly to the provider
///
/// For tests that use default credentials:
///   PULUMI_ACCESS_TOKEN  - Pulumi access token read automatically by EscClient.CreateDefault()
///                          (or a logged-in Pulumi CLI session)
/// </summary>
public class EscConfigurationIntegrationTests
{
    private static string? Org => System.Environment.GetEnvironmentVariable("ESC_TEST_ORG");
    private static string? Project => System.Environment.GetEnvironmentVariable("ESC_TEST_PROJECT");
    private static string? Env => System.Environment.GetEnvironmentVariable("ESC_TEST_ENVIRONMENT");
    private static string? ExplicitToken => System.Environment.GetEnvironmentVariable("ESC_TEST_ACCESS_TOKEN");
    private static string? DefaultToken => System.Environment.GetEnvironmentVariable("PULUMI_ACCESS_TOKEN");

    // ── With explicit token ──────────────────────────────────────────────────

    [SkippableFact]
    public void WithExplicitToken_Load_Succeeds()
    {
        Skip.If(Org is null, "ESC_TEST_ORG not set");
        Skip.If(Project is null, "ESC_TEST_PROJECT not set");
        Skip.If(Env is null, "ESC_TEST_ENVIRONMENT not set");
        Skip.If(ExplicitToken is null, "ESC_TEST_ACCESS_TOKEN not set");

        var provider = new EscConfigurationSource(new EscConfigurationOptions
        {
            Organization = Org!,
            Project = Project!,
            Environment = Env!,
            AccessToken = ExplicitToken
        }).Build(new ConfigurationBuilder());

        provider.Load();
    }

    [SkippableFact]
    public void WithExplicitToken_ExtensionMethod_ReturnsNonEmptyConfiguration()
    {
        Skip.If(Org is null, "ESC_TEST_ORG not set");
        Skip.If(Project is null, "ESC_TEST_PROJECT not set");
        Skip.If(Env is null, "ESC_TEST_ENVIRONMENT not set");
        Skip.If(ExplicitToken is null, "ESC_TEST_ACCESS_TOKEN not set");

        var config = new ConfigurationBuilder()
            .AddEscConfiguration(Org!, Project!, Env!, accessToken: ExplicitToken)
            .Build();

        Assert.NotEmpty(config.AsEnumerable().Where(kvp => kvp.Value is not null));
    }

    // ── With default credentials (PULUMI_ACCESS_TOKEN / CLI login) ───────────

    [SkippableFact]
    public void WithDefaultCredentials_Load_Succeeds()
    {
        Skip.If(Org is null, "ESC_TEST_ORG not set");
        Skip.If(Project is null, "ESC_TEST_PROJECT not set");
        Skip.If(Env is null, "ESC_TEST_ENVIRONMENT not set");
        Skip.If(DefaultToken is null, "PULUMI_ACCESS_TOKEN not set");

        var provider = new EscConfigurationSource(new EscConfigurationOptions
        {
            Organization = Org!,
            Project = Project!,
            Environment = Env!
            // AccessToken intentionally omitted — EscClient.CreateDefault() is used
        }).Build(new ConfigurationBuilder());

        provider.Load();
    }

    [SkippableFact]
    public void WithDefaultCredentials_ExtensionMethod_ReturnsNonEmptyConfiguration()
    {
        Skip.If(Org is null, "ESC_TEST_ORG not set");
        Skip.If(Project is null, "ESC_TEST_PROJECT not set");
        Skip.If(Env is null, "ESC_TEST_ENVIRONMENT not set");
        Skip.If(DefaultToken is null, "PULUMI_ACCESS_TOKEN not set");

        var config = new ConfigurationBuilder()
            .AddEscConfiguration(Org!, Project!, Env!)
            .Build();

        Assert.NotEmpty(config.AsEnumerable().Where(kvp => kvp.Value is not null));
    }
}
