[![](https://img.shields.io/nuget/v/soenneker.hangfire.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.hangfire.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.hangfire.util/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.hangfire.util/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.hangfire.util.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.hangfire.util/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.hangfire.util/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.hangfire.util/actions/workflows/codeql.yml)

# Soenneker.Hangfire.Util

A general-purpose, reusable utility class for managing Hangfire background jobs.

## Install

```bash
dotnet add package Soenneker.Hangfire.Util
```

## Quick start

```csharp
using Soenneker.Hangfire.Util.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddHangfireUtilAsSingleton();
```

Adds `IHangfireUtil` as a singleton service.

## What you get

- `IHangfireUtil` — A general-purpose, reusable utility class for managing Hangfire background jobs.
- `HangfireUtilRegistrar` — A general-purpose, reusable utility class for managing Hangfire background jobs.
- `HangfireUtilOptions` — Represents the hangfire util options.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IHangfireUtil.DeleteFailedJobs()` | Deletes failed Hangfire jobs based on filtering options. Logs unhandled jobs if enabled. | Returns no value; the requested change is complete when the method returns. |
| `IHangfireUtil.DeleteFailedJobsSilently()` | Deletes all failed Hangfire jobs without logging unhandled ones. | Returns no value; the requested change is complete when the method returns. |
| `IHangfireUtil.DeleteSucceededJobs()` | Deletes succeeded Hangfire jobs based on filtering options. | Returns no value; the requested change is complete when the method returns. |
| `IHangfireUtil.DeleteExistingRecurringJobs()` | Removes all currently scheduled recurring Hangfire jobs. | Returns no value; the requested change is complete when the method returns. |
| `HangfireUtilRegistrar.AddHangfireUtilAsSingleton(services)` | Adds `IHangfireUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `HangfireUtilRegistrar.AddHangfireUtilAsScoped(services)` | Adds `IHangfireUtil` as a scoped service. | The same service collection, so additional registrations can be chained. |
| `HangfireUtilOptions.BatchSize` | Gets or sets batch size. | Gets or sets batch size. |
| `HangfireUtilOptions.ShouldDeleteFailedJob` | Determines whether a failed job should be deleted. | Determines whether a failed job should be deleted. |
| `HangfireUtilOptions.ShouldDeleteSucceededJob` | Determines whether a succeeded job should be deleted. | Determines whether a succeeded job should be deleted. |
| `HangfireUtilOptions.NotifyOnUnhandledFailedJobs` | Gets or sets a value indicating whether notify on unhandled failed jobs. | Gets or sets a value indicating whether notify on unhandled failed jobs. |
