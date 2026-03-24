# Pulumi ESC Configuration Provider for .NET

A `Microsoft.Extensions.Configuration` provider that loads configuration from [Pulumi ESC](https://www.pulumi.com/docs/esc/) (Environments, Secrets, and Configuration).

## Installation

```console
dotnet add package Pulumi.Esc.Extensions.Configuration
```

## Requirements

- .NET 8.0 or later
- A Pulumi account with an ESC project and environment

## Usage

### Register the provider

Using named parameters:

```csharp
var config = new ConfigurationBuilder()
    .AddEscConfiguration(
        organization: "my-org",
        project: "my-project",
        environment: "production")
    .Build();
```

Using an options object:

```csharp
var config = new ConfigurationBuilder()
    .AddEscConfiguration(new EscConfigurationOptions
    {
        Organization = "my-org",
        Project = "my-project",
        Environment = "production"
    })
    .Build();
```

### Authentication

By default the provider authenticates using `EscClient.CreateDefault()`, which reads the `PULUMI_ACCESS_TOKEN` environment variable or a logged-in Pulumi CLI session.

To supply a token explicitly:

```csharp
builder.AddEscConfiguration(
    organization: "my-org",
    project: "my-project",
    environment: "production",
    accessToken: "pul-xxxxxxxx");
```

Or via `EscConfigurationOptions.AccessToken`.

## Configuration key mapping

ESC values are mapped to standard .NET configuration keys:

| ESC value shape | .NET key |
|---|---|
| Flat string/number | `keyName` |
| Nested object | `parent:child` |
| Array | `key:0`, `key:1`, ... |

Keys are case-insensitive, consistent with the `IConfiguration` convention.

## Running tests

### Unit tests

```console
dotnet test tests/Pulumi.Esc.Extensions.Configuration.Tests
```

### Integration tests

Integration tests run against a live ESC environment. Set the following environment variables before running:

| Variable | Description |
|---|---|
| `ESC_TEST_ORG` | Pulumi organisation name |
| `ESC_TEST_PROJECT` | ESC project name |
| `ESC_TEST_ENVIRONMENT` | ESC environment name |
| `ESC_TEST_ACCESS_TOKEN` | Token for explicit-token tests (optional) |
| `PULUMI_ACCESS_TOKEN` | Token for default-credential tests (optional) |

Tests that are missing their required variables are automatically skipped.

```console
dotnet test tests/Pulumi.Esc.Extensions.Configuration.IntegrationTests
```
