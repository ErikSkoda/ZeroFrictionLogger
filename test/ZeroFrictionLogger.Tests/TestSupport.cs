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
using System.Text.RegularExpressions;
using Err = ZeroFrictionLogger.Log;

namespace Test;

public static class Support
{
    private static bool FileContainsString(string filePath, string searchString)
    {
        if (!File.Exists(filePath))
            return false;

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.Contains(searchString))
                return true;
        }

        return false;
    }

    public static bool LogFileContainsString(string searchString)
    {
        return FileContainsString(Err.GetLogPathAndFilename(), searchString);
    }

    private static bool FileContainsRegex(string filePath, string pattern)
    {
        if (!File.Exists(filePath))
            return false;

        var regex = new Regex(pattern, RegexOptions.Compiled);

        foreach (var line in File.ReadLines(filePath))
        {
            if (regex.IsMatch(line))
                return true;
        }

        return false;
    }

    public static bool LogFileContainsRegex(string pattern)
    {
        return FileContainsRegex(Err.GetLogPathAndFilename(), pattern);
    }

    public static void HandleExceptionWithoutStackTraceForTestingPurpose()
    {
        try
        {
            int x = 33;
            int y = x / 0;
        }
        catch (Exception ex)
        {
            Err.HandleExceptionWithoutStackTrace(MethodBase.GetCurrentMethod()!.Name, ex.Message);
        }
    }

    public static void UseUtc()
    {
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-utc.txt");
        if (File.Exists(optOutPathAndFilename))
        {
            File.Delete(optOutPathAndFilename);
        }
    }

    public static void UseLocalTime()
    {
        string optOutPathAndFilename = Path.Combine(Err.GetAppPath(), "no-utc.txt");
        File.Create(optOutPathAndFilename).Close();
    }

    public static void AdoptIso8601UtcTimeStamp()
    {
        string opRetainNonIso8601FileSpec =
            Path.Combine(Err.GetAppPath(), "retain-non-ISO-8610-utc-timestamp.txt");
        if (File.Exists(opRetainNonIso8601FileSpec))
        {
            File.Delete(opRetainNonIso8601FileSpec);
        }
    }

    public static void RetainNonIso8601UtcTimeStamp()
    {
        string opRetainNonIso8601FileSpec =
            Path.Combine(Err.GetAppPath(), "retain-non-ISO-8610-utc-timestamp.txt");
        File.Create(opRetainNonIso8601FileSpec).Close();
    }

    public static void UseMilliSec()
    {
        string optInPathAndFilename = Path.Combine(Err.GetAppPath(), "use-millisec.txt");
        File.Create(optInPathAndFilename).Close();
    }

    public static void UseSec()
    {
        string optInPathAndFilename = Path.Combine(Err.GetAppPath(), "use-millisec.txt");
        if (File.Exists(optInPathAndFilename))
        {
            File.Delete(optInPathAndFilename);
        }
    }
}
