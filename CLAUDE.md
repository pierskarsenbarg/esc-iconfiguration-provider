# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET 8 `Microsoft.Extensions.Configuration` provider that loads configuration from [Pulumi ESC](https://www.pulumi.com/docs/esc/) (Environments, Secrets, and Configuration). Nested ESC values are flattened using `:` as the separator, matching the standard .NET configuration hierarchy.

## Commands

```bash
# Build
dotnet build

# Unit tests (no external dependencies)
dotnet test tests/Pulumi.Esc.Extensions.Configuration.Tests/

# Integration tests (require env vars — see below)
dotnet test tests/Pulumi.Esc.Extensions.Configuration.IntegrationTests/

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

## Architecture

The library (`src/Pulumi.Esc.Extensions.Configuration/`) follows the standard `Microsoft.Extensions.Configuration` provider pattern:

- **`EscConfigurationOptions`** — holds `Organization`, `Project`, `Environment`, and optional `AccessToken`
- **`EscConfigurationSource`** — implements `IConfigurationSource`; creates an `EscClient` (explicit token via `EscClient.Create(token)`, or default credentials via `EscClient.CreateDefault()`) and returns an `EscConfigurationProvider`
- **`EscConfigurationProvider`** — implements `ConfigurationProvider`; calls `OpenEnvironmentAsync` then `ReadOpenEnvironmentAsync` on the ESC SDK, then recursively flattens the returned `Dictionary<string, object?>` (nested dicts → `parent:child`, lists → `key:0`, `key:1`, etc.) into the `Data` dictionary
- **`IEscClient` / `EscClientAdapter`** — internal interface + adapter wrapping `Pulumi.Esc.Sdk.EscClient`; exists solely to allow Moq-based unit tests without hitting the real API
- **`EscConfigurationBuilderExtensions`** — extension methods on `IConfigurationBuilder` (`AddEscConfiguration`) for registration

## Tests

**Unit tests** (`tests/Pulumi.Esc.Extensions.Configuration.Tests/`) mock `IEscClient` with Moq — no network required.

**Integration tests** (`tests/Pulumi.Esc.Extensions.Configuration.IntegrationTests/`) hit a real ESC environment. They use `Xunit.SkippableFact` and skip automatically when required env vars are absent:

| Variable | Purpose |
|---|---|
| `ESC_TEST_ORG` | Pulumi organisation name |
| `ESC_TEST_PROJECT` | ESC project name |
| `ESC_TEST_ENVIRONMENT` | ESC environment name |
| `ESC_TEST_ACCESS_TOKEN` | Token for explicit-token tests |
| `PULUMI_ACCESS_TOKEN` | Token for default-credentials tests |
