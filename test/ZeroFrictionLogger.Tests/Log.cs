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

using System.Reflection;
using System.Text;
using System.IO; //required to make Path.DirectorySeparatorChar #compliant with #legacy .Net
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
//using System.Reflection.Metadata.Ecma335; //required to make AppContext.BaseDirectory
//                                          //#compliant with #legacy .NET Core 2.1

//namespace ZeroFrictionLogger; // Not #compliant with #legacy .Net Core 2.1 (Out of Service)
namespace ZeroFrictionLogger // #compliant with #legacy Net Core 2.1 and .Net Core 8.0 LTS
{
    public static class Log
    {
        private static bool _debugEnabled = true;
        private static bool _infoEnabled = true;
        private static bool _warnEnabled = true;
        private static bool _speedBlinkEnabled = true;
        private static bool _utcEnabled = true;
        private static string _appName;

        /// <summary>
        /// Visual markers, grepable sentinel tags to catch an
        /// uninitialized state and to prevent silent errors.
        /// </summary>
        /// <returns>#zilch #iota #diddly squat</returns>
        public static string Zilch() => "#zilch #iota #diddly squat";

        /// <summary>
        /// Checks whether a value equals Zilch()
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <returns>boolean</returns>
        public static bool IsZilch(string value) => value == Zilch();

        /// <summary>
        /// Checks whether a value is different from Zilch()
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <returns>boolean</returns>
        public static bool NotZilch(string value) => value != Zilch();

        /// <summary>
        /// Visual markers, grepable sentinel tag to catch an
        /// uninitialized state and to prevent silent errors.
        /// </summary>
        /// <returns>#null-value</returns>
        public static string NullExpression() => "#null-value";

        /// <summary>
        /// Returns the passed value unless it's null, in which case it returns
        /// sentinel tag "#null-value".
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <returns>value or #null-value</returns>
        public static string ReplaceNull(string value) => value ?? NullExpression();

        /// <summary>
        /// Returns the passed value unless it's null or empty
        /// in which case it returns #zilch #iota #diddly-squat.
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <returns>value or #zilch #iota #diddly-squat</returns>
        public static string ReplaceNullOrEmpty(string value)
            => string.IsNullOrEmpty(value) ? Zilch() : value;

        /// <summary>
        /// Visual marker, grepable sentinel value for replacing data to be redacted.
        /// </summary>
        /// <returns>value or #redacted #audit</returns>
        public static string RedactedExpression() => "#redacted " + GetAuditTag();

        /// <summary>
        /// Checks whether an expression equals RedactedExpression() #redacted #audit.
        /// Changed from private to public for unit testing.
        /// Returns false when value is null.
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <returns>boolean</returns>
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
        /// <returns>[context (UNREDACTED!)] + #redacted #audit</returns>
        public static string ExplicitlyRedactAndMarkValue(string context, string value)
        {
            if (String.IsNullOrEmpty(context))
            {
                return "[host app passed no context] " + RedactedExpression();
            }
            else
            {
                return "[" + context + "] " + RedactedExpression();
            }
        }

        /// <summary>
        /// Returns log file extension for use by host application for handling log file retention.
        /// </summary>
        /// <returns>.log</returns>
        public static string GetLogFileExtension() => ".log";

        /// <summary>
        /// Returns the app name passed to InitialiseErrorHandling.
        /// In case null or empty was passed the logger will try up to four fall back 
		/// methods to determine the host app name. Each attempt will be validated
		/// and skipped if the result is `null`, `empty string` contains `dotnet`, 
		/// `xunit`, `testhost`,  `zerofrictionlogger`, `zilch` or `.`.
        /// The fourth and final fallback is to return the hard coded expression `app`.
        /// </summary>
        /// <returns>host app name</returns>
		[MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetAppName() => _appName;

        /// <summary>
        /// Returns host app path using System.AppContext.
        /// </summary>
        /// <returns>host app path</returns>
        public static string GetAppPath()
            => AppContext.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Returns app path and app name based logfile name.
        /// </summary>
        /// <returns>logfile path and filename</returns>
        public static string GetLogPathAndFilename()
            => Path.Combine(GetAppPath(), GetAppName() + GetLogFileExtension()); //#legacy proof

        /// <summary>
        /// Converts expression (for instance method name) in pascal case
        /// into a sentence (hopefully documenting the method).
        /// Replaces a call to the Humanizer package to remain at zero dependencies.
        /// </summary>
        /// <param name="value">expression passed</param>
        /// <returns>sentence</returns>
        public static string ConvertPascalCaseToSentence(string value)
        {
            return PascalToSentence.ConvertPascalCaseToSentence(value); // zero #dependencies
        }

        /// <summary>
        /// Logs message at DEBUG level. Opt out by adding marker file:
        /// no-debug.txt in host app path, checked during log initialisation.
        /// Writes data instantly without caching.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogDebug(string msg)
        {
            if (_debugEnabled) LogMessage($"[DEBUG] {msg}");
        }

        /// <summary>
        /// Logs message at INFO level. Opt out by adding marker file:
        /// no-info.txt in host app path, checked during log initialisation.
        /// Writes data instantly without caching.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
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
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
        /// </summary>
        /// <param name="msg"></param>
        public static void LogWarning(string msg)
        {
            if (_warnEnabled) LogMessage($"[WARN] {msg}");
        }

        /// <summary>
        /// Logs message at ERROR level.
        /// Writes data instantly without caching.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogError(string msg) => LogMessage($"[ERROR] {msg}");

        /// <summary>
        /// Logs message at FATAL level.
        /// Writes data instantly without caching.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogFatal(string msg) => LogMessage($"[FATAL] {msg}");

        /// <summary>
        /// Logs message at AUDIT level. Also writes grepable sentinel tag #audit for
        /// extracting an audit trail from log. The value lies in use when making
        /// calls to external processes, url's or API's in the host application
        /// providing a means to extract an audit trail from log using a grepper tool.
        /// Allows to provide audit trail reports without/before building one in the host app.
        /// Writes data instantly without caching.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
        /// </summary>
        /// <param name="msg">expression passed</param>
        public static void LogAudit(string msg) => LogMessage($"[AUDIT] {msg} #audit");

        /// <summary>
        /// Bread-and-butter method for handling exceptions in the try-catch block.
        /// Logs at ERROR level. Also writes a speedblink message by default. Opt-out
        /// by including marker file: no-speedblink.txt in host app path,
        /// checked during log initialisation.
        /// Writes data to log instantly without caching.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
        /// </summary>
        /// <param name="methodName">using reflection: `MethodBase.GetCurrentMethod().Name`</param>
        /// <param name="errMsg">exception message from catch block, `ex.Message`</param>
        /// <param name="stackTrace">stack trace from catch block, `ex.StackTrace`stack trace from catch block, `ex.StackTrace`</param>
        public static void HandleException(string methodName, string errMsg, string stackTrace)
        {
            ShowExceptionOnConsole(methodName, errMsg);
            ShowExceptionInLog(methodName, errMsg);
            LogError("tech info: " + FormatExceptionMessage(ReplaceNull(errMsg),
                ReplaceNull(stackTrace)));
        }

        /// <summary>
        /// If you as a host app dev suspect sensitive data might leak into logfile thru
        /// the exception stack trace message, use HandleExceptionWithoutStackTrace in
        /// the try-catch block. Works similar to it's cousin HandleException but replaces
        /// the stack trace message with grepable sentinel tags #redacted #audit.
        /// Note: logger contains no logic for magically auto-hiding sensitive information.
        /// As host app dev you remain responsible to prevent sensitive data from entering logs.
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
        /// In case the host appname passed to InitiliaseErrorHandling in null or empty 
		/// the logger will try up to four fall back 
		/// methods to determine the host app name. Each attempt will be validated
		/// and skipped if the result is `null`, `empty string` contains `dotnet`, 
		/// `xunit`, `testhost`,  `zerofrictionlogger`, `zilch` or `.`.
        /// The fourth and final fallback is to return the hard coded expression `app`.			
        /// </summary>
        /// <param name="appName">appName passed explicitly from the host app 
		/// using reflection or as string.</param>		
        public static void InitialiseErrorHandling(string appName)
        {
            ProcessAppName(appName);
            Console.WriteLine("Start: " + GetAppName());
            CheckOptions();

            if (CanWriteToLogFile())
            {
                LogAudit(string.Format("Start log initialisation for app: {0}", GetAppName()));

                LogAudit("debug enabled = " + _debugEnabled.ToString() +
                    " due to presence/absence of no-debug.txt in app path at initialisation");
                LogAudit("info enabled = " + _infoEnabled.ToString() +
                    " due to presence/absence of no-info.txt in app path at initialisation");
                LogAudit("warn enabled = " + _warnEnabled.ToString() +
                    " due to presence/absence of no-warn.txt in app path at initialisation");
                LogAudit("utc enabled = " + _utcEnabled.ToString() +
                    " due to presence/absence of no-utc.txt in app path at initialisation");
                LogAudit("speedblink icon enabled = " + _speedBlinkEnabled.ToString() +
                    " due to presence/absence of no-speedblink.txt in app path at initialisation");
                LogDebug("double check loglevel debug");
                LogInfo("double check loglevel info");
                LogWarning("double check loglevel warn");
                LogAudit("Gentle reminder: levels [ERROR], [FATAL] and [AUDIT] " +
                    "can not be disabled.");
                LogAudit("Log initialisation complete.");
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
        /// <returns>boolean</returns>
        public static bool IsAppNameOK(string appName)
        {
            bool ok = true;
            if (appName == null) appName = "";
            if (appName?.Length == 0) ok = false;
            if (appName.ToLower().Contains("dotnet")) ok = false;
            if (appName.ToLower().Contains("xunit")) ok = false;
            if (appName.ToLower().Contains("testhost")) ok = false;
            if (appName.ToLower().Contains("zerofrictionlogger")) ok = false;
            if (appName.ToLower().Contains("zilch")) ok = false;
            if (appName.ToLower().Contains(".")) ok = false;
            return ok;
        }

        /// <summary>
        /// Public for unit test only.
        /// </summary>
        /// <returns></returns>
        public static string FallBackExecutingAssembly()
            => Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Public for unit test only.
        /// </summary>
        /// <returns></returns>
        public static string FallBackCallingAssembly()
            => Assembly.GetCallingAssembly().GetName().Name;

        /// <summary>
        /// Public for unit test only.
        /// </summary>
        /// <returns></returns>
        public static string FallBackEnvCmdLineArgsZero()
            => Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

        private static string FallBackAppHardCoded() => "app";

        private static void ProcessAppName(string appName)
        {
            if (IsAppNameOK(appName))
            {
                _appName = appName;
            }
            else
            {
                string fallBack = FallBackExecutingAssembly();
                if (!IsAppNameOK(fallBack)) fallBack = FallBackCallingAssembly();
                if (!IsAppNameOK(fallBack)) fallBack = FallBackEnvCmdLineArgsZero();
                if (!IsAppNameOK(fallBack)) fallBack = FallBackAppHardCoded();
                _appName = fallBack;
            }
        }

        private static void CheckOptions()
        {
            _utcEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-utc.txt"));
            _debugEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-debug.txt"));
            _infoEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-info.txt"));
            _warnEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-warn.txt"));
            _speedBlinkEnabled = !File.Exists(Path.Combine(GetAppPath(), "no-speedblink.txt"));
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
            => string.Format(" + {0} + {0} + {0} + {0} + {0} + {0} + {0} +", GetExceptionTag());

        // private static readonly object LockObject = new(); // not #legacy #compliant
        private static readonly object LockObject = new object(); // Works for .Net Core 2.1 + 8.0

        private static string GetTimeStamp()
            => _utcEnabled
                ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        private static void WriteLineToLog(string logLine)
        {
            try
            {
                string msg = $"{GetTimeStamp()} {logLine}{Environment.NewLine}";
                // UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: false); //!#legacy
                UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

                lock (LockObject)
                {
                    File.AppendAllText(GetLogPathAndFilename(), msg, encoding);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(logLine);
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
            // StreamWriter file = new(GetLogPathAndFilename()); // not #legacy #compliant
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
