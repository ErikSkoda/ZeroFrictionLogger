# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [0.1.0] - 2025-07-11
- upload of initial README.md

## [0.1.1] - 2025-07-12
- update of README.md
- upload of COPYRIGHT.md
- upload of CONTRIBUTING.md
- upload of CODE_OF_CONDUCT.md
- upload of CHANGELOG.md

## [0.1.2] - 2025-07-14
- Documentation updates for consistency between Nuget and GitHub

## [0.1.3] - 2025-07-14
- Table with opt-out customization added

## [0.1.4] - 2025-07-15
- Markdown formatting of README for tables, source code, output log, full documentation

## [0.1.5] - 2025-07-15
- Moved chapter Grepable markers closer to the top since it contains a tip you can use right now.

## [0.1.6] - 2025-07-15
- Chapter on Speedblink Testing is pending attribution confirmation

## [0.9.0] - 2025-07-21
- unit test project including logger source code released on GitHub

## [0.9.3] - 2025-07-24
🧪 **Pre-release defect discovered**
- Unit testing exposed a defect directly **before going live**: all unit tests passed when running from source but some broke when running against the locally installed NuGet package.
- The `GetAppName()` method returned the **logger's own DLL name** instead of the **host app name**.

🛠️ **Fix and design decision**
- `InitialiseErrorHandling` has been updated to accept the host app name.
- Host app devs can pass the app name explicitly using reflection or a string.
- If empty or null, the logger uses **four fallback mechanisms** to resolve the host app name:
1. `Assembly.GetExecutingAssembly()`
2. `Assembly.GetCallingAssembly()`
3. `Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0])`
4. Fallback to hard coded expression: `"app"`

Each fallback is validated and skipped if the result:
- is `null`
- empty string
- contains `"dotnet"`, `"xunit"`, `"testhost"`, `"zerofrictionlogger"`, `"zilch"` or a `"."`

This approach combines **maximum developer control** with **sensible defaults**, without sacrifice to compatibility - dll or exe, Windows, Linux, MacOS, .NET 6/8+ and previous versions, Mono/Xamarin, Native AOT, Blazor etc.

🚧 **No NuGet dll**
- When unit testing from source `GetAppName()` correctly returned `"TestLogger"` (unit test host app name)
- When unit testing against a locally installed packaged dll all fallbacks except the last one failed, returning `"app"`.
- Considered and rejected a 5th fall back - using host app installation folder name as proxy for app name.
- Unit testing fallbacks from source also revealed `FallBackEnvCmdLineArgsZero` returned `testhost` instead of `TestLogger`.

:pushpin: **Summary**
- Version 0.9.3 **will not be published on NuGet** as a packaged dll.
- Version 0.9.3 **full source code is shared on GitHub** for the sake of transparency
- *A solution favoring simple over clever is in the works.*

:arrows_counterclockwise: **Updates**
- `Log.cs`
- `TestCases.cs`
- `README.md`
- `CHANGELOG.md`

## [0.9.4] - 2025-07-26
🧪 **Fall back methods not working as intended**
The 0.9.3 fix worked to an extent: the reflection-based fall back methods for retrieving host app name introduced worked fine when running from source, but not when using the packaged dll. Only the fourth and final fall back - using the hard coded `"app"` worked reliably.

🛠️ **Update and design decision**
Host app devs can still pass the host app name to `InitialiseErrorHandling`, either via reflection or as string. If the host app name passed is `null`, empty or contains `dotnet`, `xunit`, `testhost`, `zerofrictionlogger`, `zilch` or a `.` (period), the logger falls back to a single hardcoded value: `"app"`.

:pushpin: **Summary**
- *As host app dev you can pass the host app name to `InitialiseErrorHandling`, If not  the logger defaults to `"app"`.* This implementation values simple over clever while maximizing portability across `exe` and `dll` contexts, regardless of OS or platform - including Windows, Linux, MacOS, Mono/Xamarin, Native AOT, Blazor, Unity, etc. The library targets `.NETStandard2.1`.

:arrows_counterclockwise: **Updates**
- `Log.cs`
- `TestCases.cs`
- `README.md`
- `CHANGELOG.md`

## [0.9.5] - 2024-07-29
- As host app dev you can pass the host app name to `InitialiseErrorHandling`. If the passed app name is: `null`, `empty string` or contains `dotnet`, `xunit`, `testhost`,  `zerofrictionlogger`, `zilch` or `.`, then the logger defaults to `"app"`.

:arrows_counterclockwise: **Updates**
- `Log.cs`
- `TestCases.cs`
- `README.md`
- `CHANGELOG.md`
