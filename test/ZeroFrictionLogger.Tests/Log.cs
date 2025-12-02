// MIT License
// 
// Copyright (c) 2025 Erik Skoda
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Globalization;
using System.IO; //required to make Path.DirectorySeparatorChar #compliant with #legacy .Net
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZeroFrictionLogger // {} is #compliant with #legacy Net Core 2.1 and .Net Core 8.0 LTS
{
    /// <summary>
    /// ZeroFrictionLogger core class.
    /// </summary>
    public static class Log
    {
        private static bool _traceEnabled = false;
        private static bool _debugEnabled = true;
        private static bool _infoEnabled = true;
        private static bool _warnEnabled = true;
        private static bool _speedBlinkEnabled = true;
        private static bool _utcEnabled = true;
        private static bool _milliSecEnabled = false;
        private static bool _retainNonIso8601UtcTimeStamp = false;
        private static string _appName = "";

        /// <summary>
        /// Marker to be checked in unit tests to prove
        /// unit tests are indeed using the expected version of the DLL.
        /// </summary>
        /// <Returns>build nr of logger to be published</Returns>
        public static string LoggerVersion() => "1.1.1";

        /// <summary>
        /// Visual markers, grepable sentinel tags to catch an
        /// uninitialized state and to prevent silent errors.
        /// </summary>
        /// <Returns>#zilch #iota #diddly squat</Returns>
        public static string Zilch() => "#zilch #iota #diddly squat";

        /// <summary>
        /// Checks whether a value equals Zilch()
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <Returns>boolean</Returns>
        public static bool IsZilch(string value) => value == Zilch();

        /// <summary>
        /// Checks whether a value is different from Zilch()
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <Returns>boolean</Returns>
        public static bool NotZilch(string value) => value != Zilch();

        /// <summary>
        /// Visual markers, grepable sentinel tag to catch an
        /// uninitialized state and to prevent silent errors.
        /// </summary>
        /// <Returns>boolean</Returns>
        public static string NullExpression() => "#null-value";

        /// <summary>
        /// Returns the passed value unless it's null, in which case it Returns
        /// sentinel tag "#null-value".
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <Returns>value or #null-value</Returns>
        public static string ReplaceNull(string value) => value ?? NullExpression();

        /// <summary>
        /// Returns the passed value unless it's null or empty
        /// in which case it returns #zilch #iota #diddly-squat.
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <Returns>value or #zilch #iota #diddly-squat</Returns>
        public static string ReplaceNullOrEmpty(string value)
            => string.IsNullOrEmpty(value) ? Zilch() : value;

        /// <summary>
        /// Visual marker, grepable sentinel value for replacing data to be redacted.
        /// </summary>
        /// <Returns>value or #redacted #audit</Returns>
        public static string RedactedExpression() => "#redacted " + GetAuditTag();

        /// <summary>
        /// Checks whether an expression equals RedactedExpression() #redacted #audit.
        /// Changed from private to public for unit testing.
        /// Returns false when value is null.
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <Returns>boolean</Returns>
        public static bool IsRedacted(string value) => value == RedactedExpression();

        /// <summary>
        /// Method replacing the value passed with visible, grepable markers #redacted #audit.
        /// Value is in explicitly hinding sensitive data and providing the audit trail
        /// to prove it.
        /// Example usage:
        /// <example>
        /// ZeroFrictionLogger.Log.LogInfo("log in using uid: " + uid +
        ///     " and pwd: " + Err.ExplicitlyRedactAndMarkValue("password", pwd));
        /// </example>
        /// <remarks>
        /// The method is null safe, still returns the markers as expected when value is null.
        /// also returns the markers as expected when context is null, replacing context with
        /// a missing context message.
        /// WARNING: while sensitive data passed in value is redacted,
        /// sensitive data passed in context is absolutely NOT redacted!
        /// </remarks>
        /// </summary>
        /// <param name="context">UNREDACTED context info for prefixing redaction markers</param>
        /// <param name="value">Value replaced with a redaction marker</param>
        /// <Returns>[context (UNREDACTED!)] + #redacted #audit</Returns>
        public static string ExplicitlyRedactAndMarkValue(string context, string value)
        {
            string result;
            if (String.IsNullOrEmpty(context))
            {
                result = "[host app passed no context] " + RedactedExpression();
            }
            else
            {
                if (String.IsNullOrEmpty(value))
                {
                    result = "[" + context + "] " + RedactedExpression() +
                             " host app passed null or empty value. " + Zilch();
                }
                else
                {
                    int len = value.Length;
                    string asterisk = new string('*', len); // works with.NET core 2.1
                    result = "[" + context + "] " + RedactedExpression() + " " + asterisk;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns log file extension for use by host application for handling log file retention.
        /// </summary>
        /// <Returns>.log</Returns>
        public static string GetLogFileExtension() => ".log";

        /// <summary>
        /// Returns the app name passed to InitialiseErrorHandling. If the passed app name
        /// is: `null`, `empty string` or contains `dotnet`, `xunit`, `testhost`,
        /// `zerofrictionlogger`, `zilch` or `.`, then the logger defaults to `"app"`.
        /// </summary>
        /// <Returns>host app name</Returns>
		[MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetAppName() => _appName;

        /// <summary>
        /// Returns host app path using System.AppContext.
        /// </summary>
        /// <Returns>host app path</Returns>
        public static string GetAppPath()
            => AppContext.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Returns app path and app name based logfile name.
        /// </summary>
        /// <Returns>logfile path and filename</Returns>
        public static string GetLogPathAndFilename()
            => Path.Combine(GetAppPath(), GetAppName() + GetLogFileExtension()); //#legacy proof

        /// <summary>
        /// Converts expression (for instance method name) in pascal case
        /// into a sentence (hopefully documenting the method).
        /// Replaces a call to the Humanizer package to remain at zero dependencies.
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <Returns>sentence</Returns>
        public static string ConvertPascalCaseToSentence(string value)
        {
            return PascalToSentence.ConvertPascalCaseToSentence(value); // zero #dependencies
        }

        /// <summary>
        /// Logs message at DEBUG level. Opt out by adding marker file:
        /// no-debug.txt in host app path, checked during log initialisation.
        /// Writes data instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogDebug(string msg)
        {
            if (_debugEnabled) LogMessage($"[DEBUG] {msg}");
        }

        /// <summary>
        /// Logs message at TRACE level. Opt out by adding marker file:
        /// no-trace.txt in host app path, checked during log initialisation.
        /// Writes data instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogTrace(string msg)
        {
            if (_traceEnabled) LogMessage($"[TRACE] {msg}");
        }

        /// <summary>
        /// Logs message at INFO level. Opt out by adding marker file:
        /// no-info.txt in host app path, checked during log initialisation.
        /// Writes data instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogInfo(string msg)
        {
            if (_infoEnabled) LogMessage($"[INFO] {msg}");
        }

        /// <summary>
        /// Logs message at WARN level. Opt out by adding marker file:
        /// no-warn.txt in host app path, checked during log initialisation.
        /// Writes data instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg"></param>
        public static void LogWarning(string msg)
        {
            if (_warnEnabled) LogMessage($"[WARN] {msg}");
        }

        /// <summary>
        /// Logs message at ERROR level.
        /// Writes data instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogError(string msg) => LogMessage($"[ERROR] {msg}");

        /// <summary>
        /// Logs message at FATAL level.
        /// Writes data instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogFatal(string msg) => LogMessage($"[FATAL] {msg}");

        /// <summary>
        /// Logs message at AUDIT level. Also writes grepable sentinel tag #audit for
        /// extracting an audit trail from log. When making calls to external processes,
        /// url's or API's in the host application this allows grepping audit trails from log.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogAudit(string msg) => LogMessage($"[AUDIT] {msg} #audit");

        /// <summary>
        /// Bread-and-butter method for handling exceptions in the try-catch block.
        /// Logs at ERROR level. Also writes a speedblink message by default. Opt-out
        /// by including marker file: no-speedblink.txt in host app path,
        /// checked during log initialisation.
        /// Writes data to log instantly without caching.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="methodName">using reflection: `MethodBase.GetCurrentMethod().Name`</param>
        /// <param name="errMsg">exception message from catch block, `ex.Message`</param>
        /// <param name="stackTrace">stack trace from catch block, `ex.StackTrace`stack trace from
        /// catch block, `ex.StackTrace`</param>
        public static void HandleException(string methodName, string errMsg, string stackTrace)
        {
            if (String.IsNullOrWhiteSpace(methodName)) methodName = "UnknownMethod";
            if (errMsg == null) errMsg = "[err msg: " + NullExpression() + "]";
            if (stackTrace == null) stackTrace = "[stack trace: " + NullExpression() + "]";
            ShowExceptionOnConsole(methodName, errMsg);
            ShowExceptionInLog(methodName, errMsg);
            LogError("tech info: " + FormatExceptionMessage(errMsg, stackTrace));
        }

        /// <summary>
        /// If you as a host app dev suspect sensitive data might leak into logfile thru
        /// the exception stack trace message, use HandleExceptionWithoutStackTrace in
        /// the try-catch block. Works similar to it's cousin HandleException but replaces
        /// the stack trace message with grepable sentinel tags #redacted #audit.
        /// Note: logger does not auto-hide sensitive data. Ensure sensitive data is handled
        /// before calling.
        /// </summary>
        /// <param name="methodName">using reflection: `MethodBase.GetCurrentMethod().Name`</param>
        /// <param name="errMsg">exception message from catch block, `ex.Message`</param>
        public static void HandleExceptionWithoutStackTrace(string methodName, string errMsg)
            => HandleException(methodName, errMsg, RedactedExpression());

        /// <summary>
        /// One stop method for starting exception handling and logging, no config needed.
        /// Attempts to create a logfile in app folder with app name and .log extension at
        /// each start of the app if possible. If write access is absent the fall back is
        /// to log to console. Failure to create or write to logfile (no write permissions)
        /// will result in an ASCII icon being displayed in console along with an explanation.
        /// If need be, redirection of console output to file in another folder
        /// using > or >> is possible on both Windows and Linux as a work around.
        /// Retention of logfiles can be achieved either by shell scripts, batch files
        /// or by the host application making a copy.
        /// In case the host appname passed to InitiliaseErrorHandling is `null`, an empty string
		/// or contains any of the expressions `dotnet`, `xunit`, `testhost`,
		/// `zerofrictionlogger`, `zilch` or `.` (not case sensitive) the logger will fall back
		/// to the hard coded expression `app`.
        /// </summary>
        /// <param name="appName">appName passed explicitly from the host app
		/// using reflection or as string.</param>
        public static void InitialiseErrorHandling(string appName)
        {
            ProcessAppName(appName);
            CheckOptions();

            if (CanWriteToLogFile())
            {
                LogAudit(string.Format(CultureInfo.InvariantCulture,
                                       "Start log initialisation for app: {0}",
                                       GetAppName()));

                LogAudit("trace enabled = " + _traceEnabled.ToString() +
                    " due to presence/absence of use-trace.txt in app path at initialisation");
                LogAudit("debug enabled = " + _debugEnabled.ToString() +
                    " due to presence/absence of no-debug.txt in app path at initialisation");
                LogAudit("info enabled = " + _infoEnabled.ToString() +
                    " due to presence/absence of no-info.txt in app path at initialisation");
                LogAudit("warn enabled = " + _warnEnabled.ToString() +
                    " due to presence/absence of no-warn.txt in app path at initialisation");
                LogAudit("utc enabled = " + _utcEnabled.ToString() +
                    " due to presence/absence of no-utc.txt in app path at initialisation");
                LogAudit("milliseconds enabled = " + _milliSecEnabled.ToString() +
                    " due to presence/absence of use-millisec.txt in app path at initialisation");
                if (!_utcEnabled)
                {
                    LogAudit("Gentle reminder: opting out of UTC time can complicate automated " +
                            "log parsing for deep diving purposes.");
                }

                LogAudit("retain version 1.0.0 more human readable non ISO-8601 UTC timestamp = " +
                    _retainNonIso8601UtcTimeStamp.ToString() + " due to presence/absence of " +
                    "retain-non-ISO-8601-utc-timestamp.txt in app path at initialisation");

                LogAudit("speedblink icon enabled = " + _speedBlinkEnabled.ToString() +
                    " due to presence/absence of no-speedblink.txt in app path at initialisation");

                LogTrace("double check loglevel TRACE is active");
                LogDebug("double check loglevel DEBUG is active");
                LogInfo("double check loglevel INFO is active");
                LogWarning("double check loglevel WARN is active");
                LogAudit("Gentle reminder: levels [ERROR], [FATAL] and [AUDIT] " +
                    "can not be disabled.");
                LogAudit("ZFL version " + LoggerVersion());
            }
        }

        /// <summary>
        /// **Public for unit test only.** Called by InitialiseErrorHandling.
        ///	Check whether the appname, passed
        /// from host app or generated by logger is appropriate.
        /// Inappropriate app names are `null`, empty string or containing `dotnet`,
        /// `xunit`, `zerofrictionlogger`, `zilch` or `.`
        /// </summary>
        /// <param name="appName">host app name passed to `InitialiseErrorHandling`</param>
        /// <Returns>boolean</Returns>
        public static bool IsAppNameOK(string appName)
        {
            if (String.IsNullOrEmpty(appName)) return false;
            string appNameLowerCase = appName.ToLower();
            string[] invalid = { "dotnet", "xunit", "testhost", "zerofrictionlogger", "zilch" };
            return !invalid.Any(appNameLowerCase.Contains) && !appName.Contains('.');
        }

        private static string GetAppNameFromHostAppOrOtherwise(string appName)
            => IsAppNameOK(appName) ? appName : "app";

        private static void ProcessAppName(string appName)
            => _appName = GetAppNameFromHostAppOrOtherwise(appName);

        private static void CheckOptions()
        {
            _utcEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-utc.txt"));
            _traceEnabled = File.Exists(Path.Combine(GetAppPath(), "use-trace.txt"));
            _debugEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-debug.txt"));
            _infoEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-info.txt"));
            _warnEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-warn.txt"));
            _speedBlinkEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-speedblink.txt"));
            _milliSecEnabled = File.Exists(Path.Combine(GetAppPath(), "use-millisec.txt"));
            _retainNonIso8601UtcTimeStamp = File.Exists(Path.Combine(GetAppPath(),
                                                        "retain-non-ISO-8601-utc-timestamp.txt"));
        }

        private static string GetExceptionTag() => "#exception";
        private static string GetSpeedBlinkExceptionOnlyBannerToLog()
            =>
               "#speedblink" + Environment.NewLine
            +  "      ___   __  __   ___    #speedblink" + Environment.NewLine
            + @"     |  _|  \ \/ /  |_  |   #speedblink" + Environment.NewLine
            + @"     | |     \  /     | |   #speedblink" + Environment.NewLine
            + @"     | |_    /  \    _| |   #speedblink" + Environment.NewLine
            + @"     |___|  /_/\_\  |___|   #speedblink" + Environment.NewLine
            +  "                            #speedblink";

        private static string GetWarningIcon()
            => Environment.NewLine
            + "      ___    _    ___" + Environment.NewLine
            + "     |  _|  | |  |_  |" + Environment.NewLine
            + "     | |    |_|    | |" + Environment.NewLine
            + "     | |_    _    _| |" + Environment.NewLine
            + "     |___|  |_|  |___|" + Environment.NewLine;

        private static string GetAuditTag() => "#audit";

        private static void ShowExceptionOnConsole(string methodName, string errMsg)
        {
            ShowSpeedBlinkExceptionOnlyBannerOnConsole();

            if (methodName != null)
            {
                Console.WriteLine($"methodName: {methodName}");
            }

            if (errMsg != null)
            {
                Console.WriteLine($"exceptionMessage: {errMsg}");
            }
        }

        private static string GetExceptionBanner()
            => string.Format(CultureInfo.InvariantCulture,
                             " + {0} + {0} + {0} + {0} + {0} + {0} +",
                             GetExceptionTag());

        private static readonly object LockObject = new object(); // Works for .Net Core 2.1 + 8.0

        private static string GetTimeStamp()
        {
            string result;
            if (_utcEnabled)
            {
                if (_retainNonIso8601UtcTimeStamp)
                {
                    if (_milliSecEnabled)
                    {
                        result = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                          CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        result = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss",
                                                          CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    if (_milliSecEnabled)
                    {
                        result = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                          CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        result = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                          CultureInfo.InvariantCulture);
                    }
                }
            }
            else
            {
                if (_milliSecEnabled)
                {
                    result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                                   CultureInfo.InvariantCulture);
                }
                else
                {
                    result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",
                                                   CultureInfo.InvariantCulture);
                }
            }
            return result;
        }

        private static void WriteLineToLog(string logLine)
        {
            try
            {
                string msg = $"{GetTimeStamp()} {logLine}{Environment.NewLine}";
                UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

                lock (LockObject)
                {
                    File.AppendAllText(GetLogPathAndFilename(), msg, encoding);
                }
            }
            catch (Exception)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                if (logLine.StartsWith("[WARN]", StringComparison.Ordinal)
                    || logLine.StartsWith("[ERROR]", StringComparison.Ordinal)
                    || logLine.StartsWith("[FATAL]", StringComparison.Ordinal))
                {
                    Console.Error.WriteLine($"{GetTimeStamp()} {logLine}");
                }
                else
                {
                    Console.WriteLine($"{GetTimeStamp()} {logLine}");
                }
            }
        }

        private static void LogMessage(string msg)
        {
            try
            {
                WriteLineToLog(ReplaceNull(msg));
            }
            catch (Exception)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine($"log to file #failed: {ReplaceNull(msg)}");
            }
        }

        private static string FormatExceptionMessage(string errMsg, string stackTrace)
        {
            if (IsRedacted(stackTrace))
            {
                return $"{errMsg} stack trace: {stackTrace}";
            }
            else
            {
                return $"{errMsg}{Environment.NewLine} " +
                       $"stack trace: {Environment.NewLine}{stackTrace}";
            }
        }

        private static void LogSpeedBlinkIconUnlessDisabled()
        {
            if (_speedBlinkEnabled) WriteLineToLog(GetSpeedBlinkExceptionOnlyBannerToLog());
        }

        private static void ShowExceptionInLog(string methodName, string errMsg)
        {
            LogSpeedBlinkIconUnlessDisabled();
            LogError(PascalToSentence.ConvertPascalCaseToSentence(methodName));
            LogError($"{errMsg} {GetExceptionTag()}");
        }

        private static void CreateLogFile()
        {
            StreamWriter file = new StreamWriter(GetLogPathAndFilename()); //OK 4 .Net Core 2.1-8.0
            file.Close();
        }

        private static bool CanCreateLogfile()
        {
            try
            {
                CreateLogFile();
                File.Delete(GetLogPathAndFilename());
                return true;
            }
            catch
            {
                Console.WriteLine(GetWarningIcon());
                Console.WriteLine($"[INFO] Could not create logfile: {GetLogPathAndFilename()}");
                Console.WriteLine("[WARN] Falling back to console.");
                return false;
            }
        }

        private static bool CanWriteToLogFile()
        {
            try
            {
                bool result = CanCreateLogfile();
                LogAudit("start log.");
                return result;
            }
            catch
            {
                Console.WriteLine(GetWarningIcon());
                Console.WriteLine($"[INFO] Could not write to logfile: {GetLogPathAndFilename()}");
                Console.WriteLine("[WARN] Falling back to console.");
                return false;
            }
        }

        private static void ShowBannerOnConsole(string banner, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(banner);
            Console.ResetColor();
        }

        private static void ShowSpeedBlinkExceptionOnlyBannerOnConsole()
        {
            Console.WriteLine("");
            ShowBannerOnConsole(GetExceptionBanner(), ConsoleColor.White);
            ShowBannerOnConsole(GetExceptionBanner(), ConsoleColor.Yellow);
            ShowBannerOnConsole(GetExceptionBanner(), ConsoleColor.Red);
            Console.WriteLine("");
        }
    }
}
