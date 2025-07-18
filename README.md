## ZeroFrictionLogger
Zero config, zero dependency exception handler and logger, logs designed for both human reading and automated processing.

Features thread safe logging, fall back to console and speedblink testing. Supports .Net Core 2.1 and 8.0 LTS, builds and runs on Windows and Linux. Writes to log without caching.

140-ish lines of executable code, 500+ due to whitespace and comments (MIT-license and for intellisense XML)

**Note:** NuGet and GitHub pre-release documentation roll out.

## Why

### No filtering, no surprises
Way back I had to use a mandatory logger. The logger refused to log stack traces with curly braces. I wanted simple not clever. No surprises.

### No additional vulnerability
Way back the test tool I was working on started crashing intermittently at midnight. The mandatory logger included a midnight roll over feature for log file retention. The documentation was elaborate. Looked for but did not find info on how to opt out of the midnight roll over feature. The need was for simple not clever. No additional vulnerability.

### No caching, no loss of data
A while ago the test tool I was using started crashing at 22:00. The tooling failed where it had never failed before, performing trivial tasks during startup which had worked thousands of times for many years. The consistent timing (22:00 plus a few sec) provided a clue the tool might be killed by an external process. Windows Event Viewer confirmed the suspicion. After reaching out to security it turned out they had introduced a new audit tool capable of killing "suspicious" processes. We solved the issue with a slightly changed regression test start time and adding the tool to an allow list. No caching meant no loss of data. The time stamp of the last successfully written log line provided info for root cause analysis.

### Simplicity as security feature
Built the original version of the logger way before november 2021. Simplicity means no surprise behaviour through runtime injection.

## Grepable markers
The exception methods (and logAudit) add visual markers, grepable sentinel tags (#exception, #audit) to the log. This allows to extract and share for instance audit reports without/before building them in the host app. Used the approach in a test tool with #summary, #details, #metrics, #S2R (steps to reproduce) enabling provision of reports before having built them. This allowed for initial focus on 1. tool stability and 2. testware validation - a huge time saver. The approach allowed building fancy HTML reports with pie charts and styling later, with the reduced pressure of "nice to have" rather than high-pressure "must have" features, allowing for some much-needed breathing space.

*Come to think of it, you don't even need ZeroFrictionLogger to pull this off.* You can probably do it already with the logger you're using right now.

## Security - don't slam your fingers
The logger contains mechanisms to prevent leaking sensitive data to log. One of several features is HandleExceptionWithoutStackTrace, cousin of HandleException. Intended use for both is in the catch block. It is up to you as host app developer to make an informed choice to omit the stack trace from log when you suspect it might contain sensitive data.

This paragraph is like a sticker on the side of the hammer saying: Don't slam your fingers. Like the hammer the logger contains no functionality to magically auto-prevent you from slamming your fingers.

## Zero config
No config is required. No XML, no JSON, no YAML. You can start developing without ever needing any of that.

### Opt out levels DEBUG, INFO, WARN
As you move from Dev to Ops you may still want to **opt out of log levels DEBUG, INFO and WARN**.

### Three Opt Out Options
1. Comment out calls to `LogDebug`, `LogInfo`, `LogWarning`. Cumbersome and requires changes to code base.
2. Grep out [DEBUG] [INFO] and [WARN] afterwards. Possible, but requires some scripting.
3. Add marker files `no-debug.txt`, `no-info.txt` and `no-warn.txt` to the host app bin folder, checked by `InitialiseErrorHandling`.

You can still start with zero config. Can go LIVE without config. You can opt-out of log levels with minimal complexity and same code base.

The log prepared by `InitialiseErrorHandling` will tell you (and double check) whether levels DEBUG, INFO, WARN are active - and briefly inform on how to opt-out.

### No ejection seat without parachute
Gentle reminder: there is **no way to opt out** of levels [ERROR], [FATAL] and [AUDIT]. That would be like providing a pilot with an ejection seat without parachute.

While a pilot might be grateful for an ejection seat to leave a burning plane, the gratitude probably lasts longer if a parachute is included.

## Speedblink testing
*(Pending attribution confirmation.)*

## Timezone - UTC by default, local possible
Uses Utc time with format yyyy-MM-dd HH:mm:ss by default.

**Opt out of Utc time** in favor of local time by adding marker file `no-utc.txt` in host app bin folder.

## Logfile retention
The app creates the log at start, replacing any previous log. Retention of logfiles is possible using scripts, batchfiles, trusted tools or by the host app making a copy.

## log path and location
Logs are created in the host app bin folder, using the app name with .log extension.

## Quick start

### Using directive
```csharp
using System.Reflection;
using Err = ZeroFrictionLogger.Log;
```

### Initialise
```csharp
InitialiseErrorHandling();
```

### Handle exception
```csharp
catch (Exception ex)
{
    Err.HandleException(MethodBase.GetCurrentMethod().Name, ex.Message, ex.StackTrace);
}
```

### ...without stack trace
```csharp
catch (Exception ex)
{
    Err.HandleExceptionWithoutStackTrace(MethodBase.GetCurrentMethod().Name, ex.Message);
}
```

### Opt out of default behaviour, complete list
You may want to customize logger behaviour. Here is how to do it.

| Opt out of       | Marker file         | Notes                                                                 |
|------------------|---------------------|-----------------------------------------------------------------------|
| log level DEBUG  | `no-debug.txt`      | Checked once by `InitialiseErrorHandling`                            |
| log level INFO   | `no-info.txt`       | Checked once by `InitialiseErrorHandling`                            |
| log level WARN   | `no-warn.txt`       | Checked once by `InitialiseErrorHandling`                            |
| log level ERROR  | **not possible**    | Always logged                                                        |
| log level FATAL  | **not possible**    | Always logged                                                        |
| log level AUDIT  | **not possible**    | Always logged                                                        |
| speedblink text  | `no-speedblink.txt` | Checked once by `InitialiseErrorHandling`                            |
| using UTC time   | `no-utc.txt`        | Checked once by `InitialiseErrorHandling`, alternative is local time |


*`InitialiseErrorHandling` checks marker file presence in the host app bin folder.* The log starts by sharing an update on customized behaviour and briefly informs you on how to opt-out.

## UTF-8
UTF-8 Encoding without BOM should take care of compatibility with many other tools downstream.

## Fall back to console
Failure to create or write to logfile to the host app bin folder (no write permissions?) will result in logging to console. If need be, redirection of console output to file in another folder using > or >> can provide a quick fix on both Windows and Linux.

## Portability
Builds on Windows and Linux, Demo app runs on Windows and Linux. All unit tests pass on Windows and Linux. **Should** work on MacOS - which is a fancy way of saying it still needs to be confirmed.

## Compatibility
Developing, building, running and unit testing using .Net Core 8.0 LTS.
Builds and runs on Windows in a console application with .Net Core 2.1 (Out of Support)

## Scaling
The logger allows a quick start at small scale.
Scaling is possible with some opt out features, scripts and tools around the two core class modules.
Will look into post processing for integration with the dashboard tool Grafana at a later stage.

## Migration
There are **many** great and feature rich loggers out there, some including advanced telemetry. Will deliberately keep this logger simple not clever, lean and ...minimalistic. Being aware your host app may outgrow the possibilities of ZeroFrictionLogger, the best I can do to facilitate later migration to a feature rich enterprise logger is to provide transparent examples and full documentation. See documentation below.

## Example log output
```text
2025-07-14 15:10:14 [INFO] start log.
2025-07-14 15:10:14 [AUDIT] Start log initialisation for app: Test #audit
2025-07-14 15:10:14 [AUDIT] debug enabled = True due to presence/absence of no-debug.txt in app path at initialisation #audit
2025-07-14 15:10:14 [AUDIT] info enabled = True due to presence/absence of no-info.txt in app path at initialisation #audit
2025-07-14 15:10:14 [AUDIT] warn enabled = True due to presence/absence of no-warn.txt in app path at initialisation #audit
2025-07-14 15:10:14 [AUDIT] speedblink icon enabled = True due to presence/absence of no-speedblink.txt in app path at initialisation #audit
2025-07-14 15:10:14 [DEBUG] double check loglevel debug
2025-07-14 15:10:14 [INFO] double check loglevel info
2025-07-14 15:10:14 [WARN] double check loglevel warn
2025-07-14 15:10:14 [AUDIT] Gentle reminder: levels [ERROR], [FATAL] and [AUDIT] can not be disabled. #audit
2025-07-14 15:10:14 [AUDIT] Log initialisation complete. #audit
...
2025-07-14 15:10:14 [INFO] Rain in Ireland has been referred to me as liquid sunshine.
2025-07-14 15:10:14 [WARN] Animal print pants out control.
2025-07-14 15:10:14 [ERROR] Pizza with pineapple is a recoverable error.
2025-07-14 15:10:14 [FATAL] It was at that moment Nathan knew, he'd bleep-ed up.
...
2025-07-14 15:10:14 #speedblink
      ___   __  __   ___    #speedblink
     |  _|  \ \/ /  |_  |   #speedblink
     | |     \  /     | |   #speedblink
     | |_    /  \    _| |   #speedblink
     |___|  /_/\_\  |___|   #speedblink
                           .#speedblink
2025-07-14 15:10:14 [ERROR] Test exception results in pipe symbol in log
2025-07-14 15:10:14 [ERROR] Attempted to divide by zero. #exception
2025-07-14 15:10:14 [ERROR] tech info: Attempted to divide by zero. stack trace: #redacted #audit
```

## Full documentation  
Public method documentation of `log.cs`.

### Zilch  
Visual markers, grepable sentinel tags to catch an  
uninitialized state and to prevent silent errors.

**Returns:** `#zilch #iota #diddly-squat`

---

### IsZilch  
Checks whether a value equals `Zilch()`.

**Parameters:**  
- `value`

**Returns:** boolean

---

### NotZilch  
Checks whether a value is different from `Zilch()`.

**Parameters:**  
- `value`

**Returns:** boolean

---

### NullExpression  
Visual markers, grepable sentinel tag to catch an  
uninitialized state and to prevent silent errors.

**Returns:** `#null-value`

---

### ReplaceNull  
Returns the passed value unless it's null, in which case it returns  
sentinel tag `#null-value`.

**Parameters:**  
- `value`

**Returns:** `#null-value`

---

### ReplaceNullOrEmpty  
Returns the passed value unless it's null or empty,  
in which case it returns `#zilch #iota #diddly-squat`.

**Parameters:**  
- `value`

**Returns:** `#zilch #iota #diddly-squat`

---

### RedactedExpression  
Visual marker, grepable sentinel value for replacing data to be redacted.

**Returns:** `#redacted #audit`

---

### IsRedacted  
Checks whether an expression equals `#redacted #audit`.  
Changed from private to public for unit testing.  
Returns false when value is null.

**Parameters:**  
- `value`

**Returns:** boolean

---

### ExplicitlyRedactAndMarkValue  
Method replacing the value passed with visible, grepable markers `#redacted #audit`.  
Value is in explicitly hiding sensitive data and providing the audit trail to prove it.  

Example:  

    ZeroFrictionLogger.Log.LogInfo("log in using uid: " + uid +
        " and pwd: " + Err.ExplicitlyRedactAndMarkValue("password", pwd));

The method is null safe, still returns the markers as expected when value is null.  
Also returns the markers as expected when context is null, replacing context with a missing context message.

**Parameters:**  
- `context`  
- `value`

**Returns:** `[context] + #redacted #audit`

---

### GetLogFileExtension  
Returns log file extension for use by host application for handling log file retention.

**Returns:** `.log`

---

### GetAppName  
Returns the host app name using reflection. Returns `"app"` in case of a null value.

**Returns:** host app name

---

### GetAppPath  
Returns host app path using `System.AppContext`.

**Returns:** host app path

---

### GetLogPathAndFilename  
Returns app path and app name based logfile name.

**Returns:** logfile path and filename

---

### ConvertPascalCaseToSentence  
Converts expression (for instance method name) in PascalCase  
into a sentence (hopefully documenting the method).  
Replaces a call to the Humanizer package to remain at zero dependencies.

**Parameters:**  
- `value`

**Returns:** sentence

---

### LogDebug  
Logs message at DEBUG level. Opt-out by adding marker file:  
`no-debug.txt` in host app path, checked during log initialisation.  
Writes data instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `msg`

---

### LogInfo  
Logs message at INFO level. Opt-out by adding marker file:  
`no-info.txt` in host app path, checked during log initialisation.  
Writes data instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `msg`

---

### LogWarning  
Logs message at WARN level. Opt-out by adding marker file:  
`no-warn.txt` in host app path, checked during log initialisation.  
Writes data instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `msg`

---

### LogError  
Logs message at ERROR level.  
Writes data instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `msg`

---

### LogFatal  
Logs message at AUDIT level. Also writes grepable sentinel tag `#audit` for  
extracting an audit trail from log. Useful when making calls to external processes, URLs, or APIs  
in the host application, providing a means to extract audit reports via grep tools.  
Writes data instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `msg`

---

### LogAudit  
Logs message at AUDIT level. Also writes grepable sentinel tag `#audit` for  
extracting an audit trail from log. Useful when making calls to external processes, URLs, or APIs  
in the host application, providing a means to extract audit reports via grep tools.  
Writes data instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `msg`

---

### HandleException  
Bread-and-butter method for handling exceptions in try-catch blocks.  
Logs at ERROR level and also writes a speedblink message by default.  
Opt-out by including marker file: `no-speedblink.txt` in host app path,  
checked during log initialisation. Writes data to log instantly without caching.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `methodName`  
- `errMsg`  
- `stackTrace`

---

### HandleExceptionWithoutStackTrace  
If you as a host app dev suspect sensitive data might leak into logfile through  
the exception stack trace message, use `HandleExceptionWithoutStackTrace` in  
the try-catch block. Works similar to its cousin `HandleException` but replaces  
the stack trace message with grepable sentinel tags `#redacted #audit`.  

**Note:** Logger contains no logic for magically auto-hiding sensitive information.  
As host app dev you remain responsible to prevent sensitive data from entering logs.

**Parameters:**  
- `methodName`  
- `errMsg`

---

### InitialiseErrorHandling  
One-stop method for starting exception handling and logging, no config needed.  
Attempts to create a logfile in app folder with app name and `.log` extension at  
each start of the app if possible. If write access is absent the fallback is  
to log to console. Failure to create or write to logfile (no write permissions)  
will result in an ASCII icon displayed in console with explanation.  
Redirection of console output to file in another folder using `>` or `>>`  
is possible on both Windows and Linux as a workaround.  

Retention of logfiles can be achieved by shell scripts, batch files, or by  
the host application making a copy.

## Project Policies
- Please see [CONTRIBUTING.md](https://github.com/ErikSkoda/ZeroFrictionLogger/blob/main/CONTRIBUTING.md) for contribution guidelines.
- Review our [Code of Conduct](https://github.com/ErikSkoda/ZeroFrictionLogger/blob/main/CODE_OF_CONDUCT.md) to understand community standards.
