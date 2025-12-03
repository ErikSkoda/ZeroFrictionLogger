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

#nullable disable
using System.Reflection;
using Xunit;
using Err = ZeroFrictionLogger.Log; // choose using to your liking your in host app

namespace Test;

[Collection("Sequential")]

public static class TestCases // builds in both 2.1 (Out of Service) and 8.0 LTS
{
    private static readonly object LockObject = new object();

    [Fact]
    public static void HandleException_StackTraceWithCurlyBraces_LogExpressionContainsCurlyBraces()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string newLine = Environment.NewLine;
        string exStackTrace =
            "no such element: Unable to locate element: {\"method\":\"xpath\",\"selector\":" +
            "\"//td[contains(text(),'|')]\"}" + newLine + "  (Session info: chrome=55.0.2883" +
            ".87)" + newLine + "  (Driver info: chromedriver=2.26.436362 (5476ec" +
            "6bf7ccbada1734a0cdec7d570bb042aa30),platform=Windows NT 10.0.10586 x86_64)" +
            newLine + "stack trace:" + newLine + "   at OpenQA.Selenium.Remote.RemoteWebDriv" +
            "er.UnpackAndThrowOnError(Response errorResponse) in c:\\..." +
            "\\SomeClassCallingWebDriver.cs:line 93" + newLine +
            "   at OpenQA.Selenium.Remote.RemoteWebDriver.Execute(String " +
            "driverCommandToExecute, Dictionary`2 parameters) in c:\\..." +
            "\\SomeClassCallingWebDriver.cs:line 46" + newLine +
            "   at OpenQA.Selenium.Remote.RemoteWebDriver.FindElement(String mechanism, " +
            "String value) in c:\\...\\SomeClassCallingWebDriver.cs:line 77" + newLine +
            "   at OpenQA.Selenium.Remote.RemoteWebDriver.FindElementByXPath(String xpath) " +
            "in c:\\...\\SomeClassCallingWebDriver" +
            ".cs:line 64" + newLine + "   at OpenQA.Selenium.By.<>c__DisplayClasse.<XPath>" +
            "b__c(ISearchContext context) in c:\\...\\src" +
            "\\webdriver\\By.cs:line 67" + newLine + "   at App.CallsToSelenium." +
            "FindWebElement(String descriptiveID) in D:\\TFS\\" +
            "...\\SomeCallsToSelenium.cs:line 60 " + newLine +
            "Note: paths and classnames abbreviated and changed from original stack trace.";

        string expected = "no such element: Unable to locate element: " +
                        "{\"method\":\"xpath\",\"selector\":\"//td[contains(text(),'|')]\"}";

        // act
        Err.HandleException("Test logging curly braces", "Some ancient error", exStackTrace);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogInfo_RainInIreland_ContainsLiquidSunshine()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-info.txt");
        File.Delete(optOutPathAndFilename);
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string info = "Rain in Ireland has been referred to me as liquid sunshine.";
        string expected = "[INFO] " + info;

        // act
        Err.LogInfo(info);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogInfo_OptOutOfLevelInfo_InfoNotLogged()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-info.txt");
        File.Create(optOutPathAndFilename).Close();
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string info = "If marker file: no-info.txt exists in app path when calling " +
            "InitialiseErrorHandling, calls to LogInfo should not result in a log entry " +
            "at [INFO] level but go ignored instead.";
        string expected = "[INFO] " + info;

        // act
        Err.LogInfo(info);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void LogInfo_OptOutOfLevelWarn_WarningNotLogged()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-warn.txt");
        File.Create(optOutPathAndFilename).Close();
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string warning = "If marker file: no-warn.txt exists in app path when calling " +
            "InitialiseErrorHandling, calls to LogWarning should not result in a log entry " +
            "at [WARN] level but go ignored instead.";
        string expected = "[WARN] " + warning;

        // act
        Err.LogWarning(warning);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void LogWarning_AnimalPrintPants_ContainsOutControl()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-warn.txt");
        File.Delete(optOutPathAndFilename);
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string warning = "Animal print pants out control";
        string expected = "[WARN] " + warning;

        // act
        Err.LogWarning(warning);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_PizzaWithPineApple_ContainsError()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple is a recoverable error.";
        string expected = "[ERROR] " + error;

        // act
        Err.LogError(error);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }


    [Fact]
    public static void LogFatal_ItWasAtThatMomentNathanKnew_ContainsBleepedUp()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string fatal = "It was at that moment Nathan knew, he'd bleep-ed up.";
        string expected = "[FATAL] " + fatal;

        // act
        Err.LogFatal(fatal);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogFatal_Contains_FATALBetweenSquareBrackets()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string fatal = "It was at that moment Nathan knew, he'd bleep-ed up.";
        string expected = "[FATAL]";

        // act
        Err.LogFatal(fatal);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogDebug_PenicillinDiscoveredByAccident_Saved80To200MillionLives()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-debug.txt");
        File.Delete(optOutPathAndFilename);
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string debug = "Penicillin, discovered by accident, " +
            "has saved between 80 and 200 million lives.";
        string expected = "[DEBUG] " + debug;

        // act
        Err.LogDebug(debug);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogTrace_FollowTheBreadCrumbs_ContainsTrace()
    {
        // arrange
        string optInPathAndFilename = Path.Combine(Err.GetAppPath(), "use-trace.txt");
        lock (LockObject)
        {
            if (!File.Exists(optInPathAndFilename))
            {
                File.Create(optInPathAndFilename).Close();
            }
        }
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string trace = "Follow the bread crumbs";
        string expected = "[TRACE]";

        // act
        Err.LogTrace(trace);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogTrace_FollowTheBreadCrumbs_ContainsBread()
    {
        // arrange
        string optInPathAndFilename = Path.Combine(Err.GetAppPath(), "use-trace.txt");
        lock (LockObject)
        {
            if (!File.Exists(optInPathAndFilename))
            {
                File.Create(optInPathAndFilename).Close();
            }
        }
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string trace = "Follow the bread crumbs";
        string expected = "bread";

        // act
        Err.LogTrace(trace);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogTrace_NoOptIn_NoTraceFound()
    {
        // arrange
        string optInPathAndFilename = Path.Combine(Err.GetAppPath(), "use-trace.txt");
        {
            if (File.Exists(optInPathAndFilename))
            {
                File.Delete(optInPathAndFilename);
            }
        }
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string trace = "No opt in no trace";
        string expected = trace;

        // act
        Err.LogTrace(trace);
        bool actual = !Support.LogFileContainsString(expected);
        
        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogDebug_OptOutOfLevelDebug_DebugNotLogged()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-debug.txt");
        File.Create(optOutPathAndFilename).Close();
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string debug = "If marker file: no-debug.txt exists in app path when calling " +
            "InitialiseErrorHandling, calls to LogDebug should not result in a log entry " +
            "at [DEBUG] level but go ignored instead.";
        string expected = "[DEBUG] " + debug;

        // act
        Err.LogDebug(debug);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void HandleExceptionWithoutStackTrace_SpeedBlinkEnabled_SpeedblinkLogged()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-speedblink.txt");
        File.Delete(optOutPathAndFilename);
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string expected = "#speedblink";

        // act
        Err.HandleExceptionWithoutStackTrace("some method name", "simulated error");
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void HandleExceptionWithoutStackTrace_SimulateError_LogContainsHashTagRedacted()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string expected = "#redacted";

        // act
        Err.HandleExceptionWithoutStackTrace("method name", "sim. error"); //stacktrace not passed
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void HandleExceptionWithoutStackTrace_ForceException_MethodNameNotFoundInLog()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string unExpected = "HandleExceptionWithoutStackTraceForTestingPurpose()";

        // act
        Support.HandleExceptionWithoutStackTraceForTestingPurpose();
        bool actual = Support.LogFileContainsString(unExpected);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void HandleExceptionWithoutStackTrace_ForceException_LineColonNotFoundInLog()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string unExpected = "line:";

        // act
        Support.HandleExceptionWithoutStackTraceForTestingPurpose();
        bool actual = Support.LogFileContainsString(unExpected);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void HandleException_ErrMsgNull_MethodNameGetsLogged()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        bool actual = false;
        string methodName = MethodBase.GetCurrentMethod().Name;

        try
        {
            // act
            int x = 33;
            int y = x / 0;
        }
        catch (Exception ex)
        {
            Err.HandleException(methodName, null, ex.StackTrace);
            actual = Support.LogFileContainsString(methodName);
        }
        finally
        {
            // assert
            Assert.True(actual);
        }
    }

    [Fact]
    public static void HandleException_StackTraceNull_StackTracePlaceHolderGetsLogged()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        bool actual = false;
        string methodName = MethodBase.GetCurrentMethod().Name;
        string expected = "[stack trace: #null-value]";

        try
        {
            // act
            int x = 33;
            int y = x / 0;
        }
        catch (Exception ex)
        {
            Err.HandleException(methodName, ex.Message, null);
            actual = Support.LogFileContainsString(expected);
        }
        finally
        {
            // assert
            Assert.True(actual);
        }
    }

    [Fact]
    public static void HandleException_MethodNameNull_MethodNameGetsLogged()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        bool actual = false;
        string methodName = MethodBase.GetCurrentMethod().Name;
        string expected = "[stack trace: #null-value]";

        try
        {
            // act
            int x = 33;
            int y = x / 0;
        }
        catch (Exception ex)
        {
            Err.HandleException(methodName, ex.Message, null);
            actual = Support.LogFileContainsString(expected);
        }
        finally
        {
            // assert
            Assert.True(actual);
        }
    }

    [Fact]
    public static void GetAppName_AssemblyGetExecutingAssembly_EqualsTestLogger()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "TestLogger";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithReflection_DoesNotContainXunit()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string unexpected = "xunit";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppName_InitialisedWithEmptyString_DoesNotContainZeroFrictionLogger()
    {
        // arrange
        Err.InitialiseErrorHandling("");
        const string unexpected = "ZeroFrictionLogger";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppName_InitialisedWithNull_DoesNotCOntainZeroFrictionLogger()
    {
        // arrange
        Err.InitialiseErrorHandling(null);
        const string unexpected = "ZeroFrictionLogger";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppName_InitialisedWithEmptyString_DoesNotContainDotNet()
    {
        // arrange
        Err.InitialiseErrorHandling("");
        const string unexpected = "DotNet";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppName_InitialisedWithNull_DoesNotContainDotNet()
    {
        // arrange
        Err.InitialiseErrorHandling(null);
        const string unexpected = "DotNet";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppName_InitialisedWithNull_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling(null);
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithEmptyString_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithTestHost_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("TestHost");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithLowerCaseDotNet_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("dotnet");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithZeroFrictionLogger_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("ZeroFrictionLogger");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithXunit_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("xunit");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithZilch_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("zilch");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithSomeHostAppNameDotExe_EqualsApp()
    {
        // arrange
        Err.InitialiseErrorHandling("somehostappname.exe");
        const string expected = "app";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithEmptyString_IsNotEmpty()
    {
        // arrange
        Err.InitialiseErrorHandling("");

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.NotEmpty(actual);
    }

    [Fact]
    public static void IsAppNameOK_RegularHostAppName_True()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK("regular-host-app-name");

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void IsAppNameOK_null_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK(null);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsAppNameOK_emptyString_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK("");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsAppNameOK_ContainsDotNet_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK("dotnet");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsAppNameOK_ContainsXunit_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK("xunit");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsAppNameOK_ContainsTestHost_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK("TESTHOST");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsAppNameOK_ContainsZeroFrictionLogger_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK("ZeroFrictionLogger");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsAppNameOK_ContainsDot_False()
    {
        // arrange // act
        bool actual = Err.IsAppNameOK(".");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithNull_IsNotEmpty()
    {
        // arrange
        Err.InitialiseErrorHandling(null);

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.NotEmpty(actual);
    }

    [Fact]
    public static void GetAppName_InitialisedWithNull_DoesNotContainZilch()
    {
        // arrange
        Err.InitialiseErrorHandling(null);
        string unexpected = "zilch";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppName_InitialisedWithEmptyString_DoesNotContainZilch()
    {
        // arrange
        Err.InitialiseErrorHandling("");
        string unexpected = "zilch";

        // act
        string actual = Err.GetAppName();

        // assert
        Assert.DoesNotContain(unexpected, actual, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public static void GetAppPath_EndsWithNet8Dot0_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string expected = "net8.0";

        // act
        string actual = Err.GetAppPath();

        // assert
        Assert.EndsWith(expected, actual);
    }

    [Fact]
    public static void InitialiseErrorHandling_LogFileExistsAfterwards_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string logfile = Err.GetLogPathAndFilename();

        // act
        bool actual = File.Exists(logfile);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void GetLogPathAndFileName_ContainsNet8Dot0_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "net8.0";

        // act
        string actual = Err.GetLogPathAndFilename();

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void GetLogPathAndFileName_ContainsAppPath_True()
    {
        // arrange
        string appPath = AppContext.BaseDirectory.TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar);

        // act
        string logPathAndFilename = Err.GetLogPathAndFilename();

        // assert
        Assert.Contains(appPath, logPathAndFilename);
    }

    [Fact]
    public static void GetLogPathAndFileName_EndsWithDotLog_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = ".log";

        // act
        string actual = Err.GetLogPathAndFilename();

        // assert
        Assert.EndsWith(expected, actual, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public static void LogInfo_NotOptedOutLogContainsInfoBetweenBrackets_True()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-info.txt");
        File.Delete(optOutPathAndFilename);
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string info = "Rain in Ireland has been referred to me as liquid sunshine.";

        // act
        Err.LogInfo(info);
        bool actual = Support.LogFileContainsString("[INFO]");

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogWarning_NotOptedOut_LogContainsWarnBetweenBrackets()
    {
        // arrange
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-warn.txt");
        File.Delete(optOutPathAndFilename);
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string warning = "Animal print pants out control.";

        // act
        Err.LogInfo(warning);
        bool actual = Support.LogFileContainsString("[WARN]");

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_LogContainsErrorBetweenBrackets_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple is a recoverable error.";

        // act
        Err.LogError(error);
        bool actual = Support.LogFileContainsString("[ERROR]");

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogAudit_LogContainsAuditTag_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);

        // act
        Err.LogAudit("call external process for reading free available memory on tool server.");
        bool actual = Support.LogFileContainsString("#audit");

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void Zilch_ContainsZilch_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#zilch";

        // act
        string actual = Err.Zilch();
        Err.LogInfo("Check whether: " + Err.Zilch() + " contains: " + expected);

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void Zilch_ContainsIota_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#iota";

        // act
        string actual = Err.Zilch();
        Err.LogInfo("Check whether: " + Err.Zilch() + " contains: " + expected);

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void Zilch_ContainsDiddlySquat_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#diddly squat";

        // act
        string actual = Err.Zilch();
        Err.LogInfo("Check whether: " + Err.Zilch() + " contains: " + expected);

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void Zilch_EqualToLiquidSunshine_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string rainInIreland = "Liquid sunshine";
        const bool expected = false;

        // act
        bool actual = Err.IsZilch(rainInIreland);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void IsZilch_HashTagZilchHashtagIotaHashtagDiddlySquat_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string zilch = "#zilch #iota #diddly squat";

        // act
        bool actual = Err.IsZilch(zilch);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void NotZilch_FootMassage_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string footMassage = "A footmassage is nothing.";

        // act
        bool actual = Err.NotZilch(footMassage);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void NotZilch_ZilchMessage_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string zilchMessage = "#zilch #iota #diddly squat";
        string expected = false.ToString();

        // act
        bool check = Err.NotZilch(zilchMessage);
        string actual = check.ToString();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void NullExpression_ContainsHashTagNull_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string hashTagNull = "#null";

        // act
        string actual = Err.NullExpression();

        // assert
        Assert.Contains(hashTagNull, actual);
    }

    [Fact]
    public static void NullExpression_ContainsNullReference_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string nullReference = "NullReference";

        // act
        string actual = Err.NullExpression();

        // assert
        Assert.DoesNotContain(nullReference, actual);
    }

    [Fact]
    public static void NullExpression_EqualsHashtagNullDashValue_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string hashTagNullValue = "#null-value";

        // act
        string actual = Err.NullExpression();

        // assert
        Assert.Equal(hashTagNullValue, actual);
    }

    [Fact]
    public static void RedactedExpression_Call_ResultContainsHashtagRedacted()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#redacted";

        // act
        string actual = Err.RedactedExpression();

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void RedactedExpression_Call_ResultContainsHashtagAudit()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#audit";

        // act
        string actual = Err.RedactedExpression();

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void IsRedacted_TiramisuRecipe_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string data = "Top secret authentic Veneto region tiramisu recipe: ...";

        // act
        bool actual = Err.IsRedacted(data);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsRedacted_HashtagRedactedHashTagAudit_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string data = "#redacted #audit";

        // act
        bool actual = Err.IsRedacted(data);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void IsRedacted_NullValue_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);

        // act
        bool actual = Err.IsRedacted(null);

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void IsRedacted_EmptyString_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);

        // act
        bool actual = Err.IsRedacted("");

        // assert
        Assert.False(actual);
    }

    [Fact]
    public static void LogAudit_AllowsGreppingAuditTrailFromLog_True()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);

        // act
        Err.LogAudit("Call customer data API customerDataService");
        Err.LogAudit("Call external process checking available memory");
        Err.LogAudit("Call external process customer debts");

        // assert
        Assert.True(Support.LogFileContainsString("API customerDataService #audit"));
        Assert.True(Support.LogFileContainsString("available memory #audit"));
        Assert.True(Support.LogFileContainsString("customer debts #audit"));
    }

    [Fact]
    public static void LogAudit_MagicallyAutoHideSensitiveData_False()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string ssnNumber = "ssn number: 123-45-6789";

        // act
        Err.LogAudit(ssnNumber);

        // assert
        Assert.False(!Support.LogFileContainsString(ssnNumber));
    }

    [Fact]
    public static void ExplicitlyRedactAndMarkValue_SensitiveData_ValueIsRedacted()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string pwdTopSecretFriedChickenRecipe = "123Pizza!";

        // act
        string actual = Err.ExplicitlyRedactAndMarkValue("pwd", pwdTopSecretFriedChickenRecipe);

        // assert
        Assert.DoesNotContain(pwdTopSecretFriedChickenRecipe, actual);
    }

    [Fact]
    public static void ExplicitlyRedactAndMarkValue_SensitiveData_ContextIsAbsolutelyNotRedacted()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string pwd = "123Pizza!";
        string contextHoldingSensitiveData = "Politican XYZ has a hair piece";

        // act
        string actual = Err.ExplicitlyRedactAndMarkValue(contextHoldingSensitiveData, pwd);

        // assert
        Assert.Contains(contextHoldingSensitiveData, actual);
    }

    [Fact]
    public static void ExplicitlyRedactAndMarkValue_PassingNullValueFlaggedWithZilch()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string expected = Err.Zilch();

        // act
        string actual = Err.ExplicitlyRedactAndMarkValue("password", null);

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void ExplicitlyRedactAndMarkValue_PassingNullValueFlaggedWithNullOrEmpty()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string expected = "null or empty";

        // act
        string actual = Err.ExplicitlyRedactAndMarkValue("password", null);

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void ExplicitlyRedactAndMarkValue_PassingNullForContextFlaggedGracefully()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        string pwd = "MyFirstPetsNameWasHermanMunst3r";
        string expected = "host app passed no context";

        // act
        string actual = Err.ExplicitlyRedactAndMarkValue(null, pwd);

        // assert
        Assert.Contains(expected, actual);
    }

    [Fact]
    public static void ReplaceNull_PassNull_ReturnsPlaceHolder()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#null-value";

        // act
        string actual = Err.ReplaceNull(null);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ReplaceNull_PassData_ReturnsData()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "Name of Quarter Pounder in France";

        // act
        string actual = Err.ReplaceNull("Name of Quarter Pounder in France");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ReplaceNullOrEmpty_PassEmptyString_ResultContainsHashtagZilch()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#zilch #iota #diddly squat";

        // act
        string actual = Err.ReplaceNullOrEmpty("");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ReplaceNullOrEmpty_PassNull_ResultContainsHashtagZilch()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "#zilch #iota #diddly squat";

        // act
        string actual = Err.ReplaceNullOrEmpty(null);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_ValueInPascalCase_ReturnsSentence()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "Convert pascal case to sentence";

        // act
        string actual = Err.ConvertPascalCaseToSentence("ConvertPascalCaseToSentence");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_ValueEndsWithAcronym_SentenceAsWell()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "Convert sentence ending with all caps HTML";

        // act
        string actual = Err.ConvertPascalCaseToSentence("ConvertSentenceEndingWithAllCapsHTML");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_ValueBeginsWithAcronym_SentenceAsWell()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = "HTML stands for hypertext etc etc";

        // act
        string actual = Err.ConvertPascalCaseToSentence("HTMLStandsForHypertextEtcEtc");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetLogFileExtension_EqualsDotLog()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string expected = ".log";

        // act
        string actual = Err.GetLogFileExtension();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void GetLogFileExtension_NotEqualDotTxt()
    {
        // arrange
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string unExpected = ".txt";

        // act
        string actual = Err.GetLogFileExtension();

        // assert
        Assert.NotEqual(unExpected, actual);
    }

    [Fact]
    public static void InitialiseErrorHandling_Call_CausesNoException()
    {
        // arrange
        bool ok = false;

        try
        {
            // act
            Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);

            ok = true;
        }
        catch (Exception)
        {
            ok = false;
        }
        finally
        {
            // assert
            Assert.True(ok);
        }
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_XML_XML()
    {
        // arrange
        string expected = "XML";

        // act
        string actual = Err.ConvertPascalCaseToSentence("XML");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_XLSizeFitsAll_XL_size_fits_all()
    {
        // arrange
        const string name = "XLSizeFitsAll";
        string expected = "XL size fits all";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_SizeXLFitsAll_Size_XL_fits_all()
    {
        // arrange
        const string name = "SizeXLFitsAll";
        string expected = "Size XL fits all";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_HTMLExpression_HTML_expression()
    {
        // arrange
        const string name = "HTMLExpression";
        string expected = "HTML expression";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_hi_Hi()
    {
        // arrange
        const string name = "hi";
        string expected = "Hi";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_X_X()
    {
        // arrange
        const string name = "X";
        string expected = "X";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_x_X()
    {
        // arrange
        const string name = "x";
        string expected = "X";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void ConvertPascalCaseToSentence_XL_XL()
    {
        // arrange
        const string name = "XL";
        string expected = "XL";

        // act
        string actual = Err.ConvertPascalCaseToSentence(name);

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void LoggerVersion_Returns_1dot1dot1()
    {
        // arrange
        string expected = "1.1.1";

        // act
        string actual = Err.LoggerVersion();

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public static void LoggerVersion_NotStartingWith_0dot()
    {
        // arrange
        string unExpected = "0.";

        // act
        string actual = Err.LoggerVersion();
        bool expected = !actual.StartsWith(unExpected);

        // assert
        Assert.True(expected);
    }

    [Fact]
    public static void LogError_UsingUtcTimeAndSeconds_Contains_ddTddColon()
    {
        // arrange
        Support.UseUtc();
        Support.UseSec();
        Support.AdoptIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "expresion with dash, two digits, T, two digits, colon expected.";
        string pattern = @"-\d{2}T\d{2}:";

        // act
        Err.LogError(error);
        bool actual = Support.LogFileContainsRegex(pattern);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingUtcTimeUsingSecondsAdoptingIso8601_CompliesWithIso8601()
    {
        // arrange
        Support.UseUtc();
        Support.UseSec();
        Support.AdoptIso8601UtcTimeStamp();

        string patternIso8601Sec = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z";
        const string error = "Pizza with pineapple detected.";

        // act
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        Err.LogError(error);
        bool actual = Support.LogFileContainsRegex(patternIso8601Sec);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingUtcTimeUsingMillisecondsAdoptingIso8601_CompliesWithIso8601()
    {
        // arrange
        Support.UseUtc();
        Support.UseMilliSec();
        Support.AdoptIso8601UtcTimeStamp();

        string patternIso8601MilliSec = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}(?:\.\d+)?Z";
        const string error = "Pizza with pineapple detected.";

        // act
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        Err.LogError(error);
        bool actual = Support.LogFileContainsRegex(patternIso8601MilliSec);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingUtcTime_UsingNonIso8601andMilliSec_NonIso8601TimeStamp()
    {
        // arrange
        Support.UseUtc();
        Support.UseMilliSec();
        Support.RetainNonIso8601UtcTimeStamp();

        string patternIso8601MilliSec = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}(?:\.\d+)?Z";
        const string error = "Pizza with pineapple detected. Non ISO8601 timestamp expected";

        // act
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        Err.LogError(error);
        bool actual = !Support.LogFileContainsRegex(patternIso8601MilliSec);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingUtcTime_UsingNonIso8601andSec_NonIso8601TimeStamp()
    {
        // arrange
        Support.UseUtc();
        Support.UseSec();
        Support.RetainNonIso8601UtcTimeStamp();

        string patternIso8601Sec = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z";
        const string error = "Pizza with pineapple detected. Non ISO8601 timestamp expected";

        // act
        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        Err.LogError(error);
        bool actual = !Support.LogFileContainsRegex(patternIso8601Sec);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingNonIso8601UtcTimeAndMilliSeconds_ContainsNo_ddTddColon()
    {
        // arrange
        Support.UseUtc();
        Support.UseMilliSec();
        Support.RetainNonIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pattern not found: dash digit digit T digit digit colon";
        string pattern = @"-\d{2}T\d{2}:";

        // act
        Err.LogError(error);
        bool actual = !Support.LogFileContainsRegex(pattern);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingNonIso8601UtcTimeAndMilliSeconds_ContainsNoZ()
    {
        // arrange
        Support.UseUtc();
        Support.UseMilliSec();
        Support.RetainNonIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Character: 'capital z' expected missing";
        string expected = "Z [";

        // act
        Err.LogError(error);

        bool actual = !Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingNonIso8601UtcTimeAndSeconds_ContainsNoddTddColon()
    {
        // arrange
        Support.UseUtc();
        Support.UseSec();
        Support.RetainNonIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pattern not found: dash digit digit T digit digit colon";
        string pattern = @"-\d{2}T\d{2}:";

        // act
        Err.LogError(error);
        bool actual = !Support.LogFileContainsRegex(pattern);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingNonIso8601UtcTimeAndSeconds_ContainsNoZSpaceBracketOpen()
    {
        // arrange
        Support.UseUtc();
        Support.UseSec();
        Support.RetainNonIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Character: 'capital z' expected missing";
        string expected = "Z [";

        // act
        Err.LogError(error);

        bool actual = !Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingUtcTimeAndSeconds_ContainsZspaceBracketOpen()
    {
        // arrange
        Support.UseUtc();
        Support.UseSec();
        Support.AdoptIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple detected.";
        string expected = "Z [";

        // act
        Err.LogError(error);
        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingUtcTimeAndMilliSeconds_ContainsZspaceBracketOpen()
    {
        // arrange
        Support.UseMilliSec();
        Support.UseUtc();
        Support.AdoptIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Characters: `capital: t<space>[` expected.";
        string expected = "Z [";

        // act
        Err.LogError(error);

        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_AdoptingIso8601_ContainsRetainFalse()
    {
        // arrange
        Support.UseUtc();
        Support.AdoptIso8601UtcTimeStamp();
        Support.UseMilliSec();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Check whether log initialisation shows moving over to the standardised utc time stamp.";
        string expected = "retain version 1.0.0 more human readable non ISO-8601 UTC timestamp = False";

        // act
        Err.LogError(error);

        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_AdoptingIso8601_ContainsRetainTrue()
    {
        // arrange
        Support.UseUtc();
        Support.RetainNonIso8601UtcTimeStamp();
        Support.UseMilliSec();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Check if log initialisation shows keeping the more readable utc time stamp";
        string expected = "retain version 1.0.0 more human readable non ISO-8601 UTC timestamp = True";

        // act
        Err.LogError(error);

        bool actual = Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingLocalTimeSecondsAndAdoptIso8601_ContainsNoZspaceBracketOpen()
    {
        // arrange
        Support.UseLocalTime();
        Support.UseSec();
        Support.AdoptIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple detected.";
        string expected = "Z [";

        // act
        Err.LogError(error);
        bool actual = !Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingLocalTimeSecondsAndButNoIso8601_ContainsNoZspaceBracketOpen()
    {
        // arrange
        Support.UseLocalTime();
        Support.UseSec();
        Support.RetainNonIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple detected.";
        string expected = "Z [";

        // act
        Err.LogError(error);
        bool actual = !Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingLocalTimeMilliSecAndAdoptIso8601_ContainsNoZspaceBracketOpen()
    {
        // arrange
        Support.UseLocalTime();
        Support.UseMilliSec();
        Support.AdoptIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple detected.";
        string expected = "Z [";

        // act
        Err.LogError(error);
        bool actual = !Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }

    [Fact]
    public static void LogError_UsingLocalTimeMilliSecAndButNoIso8601_ContainsNoZspaceBracketOpen()
    {
        // arrange
        Support.UseLocalTime();
        Support.UseMilliSec();
        Support.RetainNonIso8601UtcTimeStamp();

        Err.InitialiseErrorHandling(Assembly.GetExecutingAssembly().GetName().Name);
        const string error = "Pizza with pineapple detected.";
        string expected = "Z [";

        // act
        Err.LogError(error);
        bool actual = !Support.LogFileContainsString(expected);

        // assert
        Assert.True(actual);
    }
}
