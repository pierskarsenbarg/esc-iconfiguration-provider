using Microsoft.Extensions.Configuration;
using Moq;
using Pulumi.Esc.Sdk.Model;

namespace Pulumi.Esc.Extensions.Configuration.Tests;

public class EscConfigurationProviderTests
{
    private static EscConfigurationOptions DefaultOptions => new()
    {
        Organization = "test-org",
        Project = "test-project",
        Environment = "test-env"
    };

    private static EscConfigurationProvider BuildProvider(
        IEscClient client,
        EscConfigurationOptions? options = null)
        => new(options ?? DefaultOptions, client);

    [Fact]
    public void Load_FlatValues_PopulatesData()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), new Dictionary<string, object?>
            {
                ["apiKey"] = "secret-value",
                ["region"] = "us-east-1"
            }));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.True(provider.TryGet("apiKey", out var apiKey));
        Assert.Equal("secret-value", apiKey);
        Assert.True(provider.TryGet("region", out var region));
        Assert.Equal("us-east-1", region);
    }

    [Fact]
    public void Load_NestedObjects_FlattensWithColonSeparator()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), new Dictionary<string, object?>
            {
                ["database"] = new Dictionary<string, object?>
                {
                    ["host"] = "localhost",
                    ["port"] = 5432
                }
            }));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.True(provider.TryGet("database:host", out var host));
        Assert.Equal("localhost", host);
        Assert.True(provider.TryGet("database:port", out var port));
        Assert.Equal("5432", port);
    }

    [Fact]
    public void Load_DeeplyNestedObjects_FlattensCorrectly()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), new Dictionary<string, object?>
            {
                ["app"] = new Dictionary<string, object?>
                {
                    ["db"] = new Dictionary<string, object?>
                    {
                        ["host"] = "db.example.com"
                    }
                }
            }));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.True(provider.TryGet("app:db:host", out var host));
        Assert.Equal("db.example.com", host);
    }

    [Fact]
    public void Load_ArrayValues_FlattensWithIndexKeys()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), new Dictionary<string, object?>
            {
                ["hosts"] = new List<object?> { "host1", "host2", "host3" }
            }));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.True(provider.TryGet("hosts:0", out var h0));
        Assert.Equal("host1", h0);
        Assert.True(provider.TryGet("hosts:1", out var h1));
        Assert.Equal("host2", h1);
        Assert.True(provider.TryGet("hosts:2", out var h2));
        Assert.Equal("host3", h2);
    }

    [Fact]
    public void Load_NullValue_StoresNullInData()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), new Dictionary<string, object?>
            {
                ["optionalKey"] = null
            }));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.True(provider.TryGet("optionalKey", out var val));
        Assert.Null(val);
    }

    [Fact]
    public void Load_NullValuesFromClient_SetsEmptyData()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), (Dictionary<string, object?>?)null));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.False(provider.TryGet("anything", out _));
    }

    [Fact]
    public void Load_KeysAreCaseInsensitive()
    {
        var client = new Mock<IEscClient>();
        client
            .Setup(c => c.OpenEnvironmentAsync("test-org", "test-project", "test-env", default))
            .ReturnsAsync(("session-1", null));
        client
            .Setup(c => c.ReadOpenEnvironmentAsync("test-org", "test-project", "test-env", "session-1", default))
            .ReturnsAsync((new ModelEnvironment(), new Dictionary<string, object?>
            {
                ["ApiKey"] = "value"
            }));

        var provider = BuildProvider(client.Object);
        provider.Load();

        Assert.True(provider.TryGet("apikey", out var lower));
        Assert.Equal("value", lower);
        Assert.True(provider.TryGet("APIKEY", out var upper));
        Assert.Equal("value", upper);
    }
}

public class EscConfigurationSourceTests
{
    [Fact]
    public void Build_ReturnsEscConfigurationProvider()
    {
        var options = new EscConfigurationOptions
        {
            Organization = "org",
            Project = "proj",
            Environment = "env",
            AccessToken = "fake-token"
        };
        var source = new EscConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        Assert.IsType<EscConfigurationProvider>(provider);
    }
}

public class EscConfigurationBuilderExtensionsTests
{
    [Fact]
    public void AddEscConfiguration_WithNamedParams_AddsSingleSource()
    {
        var builder = new ConfigurationBuilder();

        builder.AddEscConfiguration("org", "proj", "env", accessToken: "tok");

        Assert.Single(builder.Sources);
        Assert.IsType<EscConfigurationSource>(builder.Sources[0]);
    }

    [Fact]
    public void AddEscConfiguration_WithOptions_AddsSingleSource()
    {
        var builder = new ConfigurationBuilder();
        var options = new EscConfigurationOptions
        {
            Organization = "org",
            Project = "proj",
            Environment = "env"
        };

        builder.AddEscConfiguration(options);

        Assert.Single(builder.Sources);
        Assert.IsType<EscConfigurationSource>(builder.Sources[0]);
    }
}
