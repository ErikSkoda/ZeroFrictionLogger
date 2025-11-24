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

using Err = ZeroFrictionLogger.Log;
using Alt = ConsoleAppWithLogging.ConversionRefactorings;
// using Humanizer;

// namespace ConsoleAppWithLogging; // not supported by older .Net versions
namespace ConsoleAppWithLogging // #compliant with both .Net Core 2.1 and .Net Core 8.0 LTS
{
    public static class Benchmark
    {
        private static string testExpression = "MeasureDurationOfOneMillionIteratonsWithACRONYMInside";

        public static void MeasureLongVersion()
        {
            PrecisionTimer.Instance.Stopwatch.Reset();
            PrecisionTimer.Instance.Stopwatch.Start();
            string result = Err.Zilch();

            for (int i = 0; i < 1000000; i++)
                result = Alt.ConversionWithoutHumanizerLongVersion(testExpression);

            PrecisionTimer.Instance.Stopwatch.Stop();
            double elapsedTimeMilliSec = PrecisionTimer.Instance.Stopwatch.ElapsedMilliseconds;

            Err.LogInfo(result);
            Err.LogInfo($"Elapsed time: {elapsedTimeMilliSec} milliseconds #long");
            Console.WriteLine($"long version (37 LOC), 1M runs: {elapsedTimeMilliSec} millisec");
        }

        public static void MeasureShortVersion()
        {
            PrecisionTimer.Instance.Stopwatch.Reset();
            PrecisionTimer.Instance.Stopwatch.Start();
            string result = Err.Zilch();

            for (int i = 0; i < 1000000; i++)
                result = Alt.ConversionWithoutHumanizerShortVersion(testExpression);

            PrecisionTimer.Instance.Stopwatch.Stop();
            double elapsedTimeMilliSec = PrecisionTimer.Instance.Stopwatch.ElapsedMilliseconds;

            Err.LogInfo(result);
            Err.LogInfo($"Elapsed time: {elapsedTimeMilliSec} milliseconds #short");
            Console.WriteLine($"short version (21 LOC), 1M runs: {elapsedTimeMilliSec} millisec");
        }

        public static void MeasureShorterVersion()
        {
            PrecisionTimer.Instance.Stopwatch.Reset();
            PrecisionTimer.Instance.Stopwatch.Start();
            string result = Err.Zilch();

            for (int i = 0; i < 1000000; i++)
                result = Alt.ConversionWithoutHumanizerShorterVersion(testExpression);

            PrecisionTimer.Instance.Stopwatch.Stop();
            double elapsedTimeMilliSec = PrecisionTimer.Instance.Stopwatch.ElapsedMilliseconds;

            Err.LogInfo(result);
            Err.LogInfo($"Elapsed time: {elapsedTimeMilliSec} milliseconds #shorter");
            Console.WriteLine($"shorter version (12 LOC), 1M runs: {elapsedTimeMilliSec} millisec");
        }

        public static void MeasureShortestVersion()
        {
            PrecisionTimer.Instance.Stopwatch.Reset();
            PrecisionTimer.Instance.Stopwatch.Start();
            string result = Err.Zilch();

            for (int i = 0; i < 1000000; i++)
                result = Alt.ConversionWithoutHumanizerShortestVersion(testExpression);

            PrecisionTimer.Instance.Stopwatch.Stop();
            double elapsedTimeMilliSec = PrecisionTimer.Instance.Stopwatch.ElapsedMilliseconds;
            Err.LogInfo(result);
            Err.LogInfo($"Elapsed time: {elapsedTimeMilliSec} milliseconds #shortest");
            Console.WriteLine($"shortest version (5 LOC), 1M runs: {elapsedTimeMilliSec} millisec");
        }

        //public static void MeasureReference()
        //{
        //    PrecisionTimer.Instance.Stopwatch.Reset();
        //    PrecisionTimer.Instance.Stopwatch.Start();
        //    string result = Err.Zilch();

        //    for (int i = 0; i < 1000000; i++)
        //        result = testExpression.Humanize(LetterCasing.Sentence);

        //    PrecisionTimer.Instance.Stopwatch.Stop();
        //    double elapsedTimeMilliSec = PrecisionTimer.Instance.Stopwatch.ElapsedMilliseconds;
        //    Err.LogInfo(result);
        //    Err.LogInfo($"Elapsed time: {elapsedTimeMilliSec} milliseconds #reference");
        //}
    }
}