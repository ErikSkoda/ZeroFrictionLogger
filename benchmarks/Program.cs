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
using Err = ZeroFrictionLogger.Log;
namespace ConsoleAppWithLogging;

internal static class Program
{
    private static void MainPayload()
    {
        Err.InitialiseErrorHandling("");

        Console.WriteLine("The original ZFL implementation of PascalCaseToSentence was 90-ish LOC.");
        Console.WriteLine();
        Console.WriteLine("Below you see the performance of 4 different refactorings implementing");
        Console.WriteLine("Robert C. Martin's idea of making methods as short as possible.");
        Console.WriteLine();
        Console.WriteLine("Initially it seemed not feasible to make the long method much shorter.");
        Console.WriteLine("Tried it anyway. The result is four implementations of " +
                            "37, 21, 13 and 5 LOC.");
        Console.WriteLine();
        Console.WriteLine("Then the question becomes: how would this affect performance?");
        Console.WriteLine();

        Benchmark.MeasureLongVersion();
        Benchmark.MeasureShortVersion();
        Benchmark.MeasureShorterVersion();
        Benchmark.MeasureShortestVersion();
        // Benchmark.MeasureReference();

        Console.WriteLine();
        Console.WriteLine("The longest version turns out to be 5% to 12% slower than the " +
            "Humanizer package.");
        Console.WriteLine("The shortest version is over twice as slow.");
        Console.WriteLine("Measurements are taken over 1M iterations.");
        Console.WriteLine();
        Console.WriteLine("Note the logger only converts pascalcase to sentence for exception " +
            "handling. This is NOT a hot path.");
        Console.WriteLine();
        Console.WriteLine("In the context of ZFL, having zero dependencies " +
            "outweighs the performance hit.");

        Console.WriteLine();
        Console.WriteLine("Since the method is not on the hot path and 1M exceptions are unrealistic,");
        Console.WriteLine("ZFL is rolling with the shortest 5 LOC refactor for now.");
        Console.WriteLine();
        Console.WriteLine("Press the [any] key to continue ;)");

        Console.ReadKey();
        Err.LogInfo("End of log.");
    }

    private static void CauseExceptionForDemoPurpose()
    {
        try
        {
#pragma warning disable RCS1118 // Mark local variable as const
            int x = 33;
#pragma warning restore RCS1118 // Mark local variable as const
            int y = x / 0;
            Console.WriteLine(x.ToString() + " divided by 0 equals" + y.ToString());
        }
        catch (Exception ex)
        {
            Err.HandleExceptionWithoutStackTrace(MethodBase.GetCurrentMethod()!.Name, ex.Message);
        }
    }

    static void Main()
    {
        try
        {
            MainPayload();
        }
        catch (Exception ex)
        {
            Err.HandleException(MethodBase.GetCurrentMethod()!.Name, ex.Message, ex.StackTrace!);
        }
    }
}