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

using System.Text;

namespace ConsoleAppWithLogging;

public static class ConversionRefactorings
{
    // logic lives below here.
    private static bool IsLogicalToAddSpaceForPascalCase(int i, string value)
        => IsUpper(value[i])
        && NotPreviousUpper(i, value)
        && NotFirst(i);

    private static bool IsLogicalToAddSpaceForAcronym(int i, string value)
        => IsUpper(value[i])
        && IsPreviousUpper(i, value)
        && IsSecondPreviousUpper(i, value)
        && NotNextUpper(i, value)
        && NotLast(i, value);

    private static bool IsLogicalToAddSpace(int i, string value)
        => IsLogicalToAddSpaceForPascalCase(i, value)
        || IsLogicalToAddSpaceForAcronym(i, value);

    private static bool IsExclusiveLogicUpperCase(int i) => IsFirst(i);

    private static bool IsExclusiveLogicLowerCase(int i, string valueWithSpacesInserted)
        => IsUpper(valueWithSpacesInserted[i])
        && IsNextLower(i, valueWithSpacesInserted)
        && IsPreviousSpace(i, valueWithSpacesInserted)
        && !IsExclusiveLogicUpperCase(i); // << explicit enforcement of exclusiveness (redundant)

    private static bool IsExclusiveLogicLeaveUnchanged(int i, string valueWithSpacesInserted)
        => !IsExclusiveLogicUpperCase(i) // << explicit enforcement of exclusiveness (must)
        && !IsExclusiveLogicLowerCase(i, valueWithSpacesInserted); //<< explicit enforcement (must)

    public static string ApplyLogicForSpacing(string value)
    {
        var sb = new StringBuilder();
        bool[] shouldIncludeSpace = GetNeedForSpace(value);
        bool[] shouldNotIncludeSpace = shouldIncludeSpace.Select(yes => !yes).ToArray();

        for (int i = 0; i < value.Length; i++)
        {
            if (shouldIncludeSpace[i]) AppendCharWithSpacePrefix(sb, value[i]);
            if (shouldNotIncludeSpace[i]) AppendCharAsIs(sb, value[i]);
        }   // ^^ enforced mutually exclusive if statements
        return sb.ToString();
    }

    public static string ApplyLogicForCasingAfterSpacing(string spaced)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < spaced.Length; i++)
        {
            if (IsExclusiveLogicUpperCase(i)) AppendCharInUpperCase(sb, spaced[i]);
            if (IsExclusiveLogicLowerCase(i, spaced)) AppendCharInLowerCase(sb, spaced[i]);
            if (IsExclusiveLogicLeaveUnchanged(i, spaced)) AppendCharAsIs(sb, spaced[i]);
        }
        return sb.ToString();
    }
    // logic lives above here ^^

    public static string ConvertPascalCaseToSentenceWithoutHumanizer(string value)
    {
        try
        {
            return ConversionWithoutHumanizerShortestVersion(value);
        }
        catch
        {
            return value;
        }
    }

    private static bool IsFirst(int i) => i == 0;
    private static bool NotFirst(int i) => !IsFirst(i);
    private static bool IsSecond(int i) => i == 1;
    private static bool IsLast(int i, string value) => i == value.Length - 1;
    private static bool NotLast(int i, string value) => !IsLast(i, value);
    private static bool IsUpper(char c) => char.IsUpper(c);
    private static bool NotUpper(char c) => !IsUpper(c);
    private static bool IsPreviousUpper(int i, string value)
        => !IsFirst(i) && char.IsUpper(value[i - 1]);
    private static bool NotPreviousUpper(int i, string value) => !IsPreviousUpper(i, value);
    private static bool IsPreviousSpace(int i, string value)
        => !IsFirst(i) && ((value[i - 1] == ' '));
    private static bool IsSpace(int i, string value)
        => !IsFirst(i) && ((value[i] == ' '));
    private static bool IsSecondPreviousUpper(int i, string value)
        => !IsFirst(i) && !IsSecond(i) && char.IsUpper(value[i - 2]);
    private static bool IsNextUpper(int i, string value)
        => NotLast(i, value) && char.IsUpper(value[i + 1]);
    private static bool IsNextLower(int i, string value)
        => NotLast(i, value) && char.IsLower(value[i + 1]);
    private static bool NotNextUpper(int i, string value) => !IsNextUpper(i, value);
    private static bool[] GetNeedForSpace(string value) =>
         value.Select((_, i) => IsLogicalToAddSpace(i, value)).ToArray();
    private static void AppendSpace(StringBuilder sb) => sb.Append(' ');
    private static void AppendCharInLowerCase(StringBuilder sb, char value)
        => sb.Append(value.ToString().ToLower());
    private static void AppendCharInUpperCase(StringBuilder sb, char value)
        => sb.Append(value.ToString().ToUpper());
    //private static void AppendUnderScoreForDebug(StringBuilder sb)
    //    => sb.Append('_');
    private static void AppendCharAsIs(StringBuilder sb, char value) => sb.Append(value);
    private static void AppendCharWithSpacePrefix(StringBuilder sb, char value)
    {
        sb.Append(' ');
        sb.Append(value);
    }

    public static string ConversionWithoutHumanizerShortestVersion(string value)
    {
        string spaced = ApplyLogicForSpacing(value);
        return ApplyLogicForCasingAfterSpacing(spaced);
    }

    public static string ConversionWithoutHumanizerShorterVersion(string value)
    {
        string spaced = ApplyLogicForSpacing(value);
        var sb = new StringBuilder();

        for (int i = 0; i < spaced.Length; i++)
        {
            if (IsExclusiveLogicUpperCase(i)) AppendCharInUpperCase(sb, spaced[i]);
            if (IsExclusiveLogicLowerCase(i, spaced)) AppendCharInLowerCase(sb, spaced[i]);
            if (IsExclusiveLogicLeaveUnchanged(i, spaced)) AppendCharAsIs(sb, spaced[i]);
        }
        return sb.ToString();
    }

    public static string ConversionWithoutHumanizerShortVersion(string value)
    {
        bool[] shouldBePrefixedWithSpace = GetNeedForSpace(value);
        bool[] shouldBeLowerCase = new bool[value.Length];

        for (int i = 0; i < value.Length; i++)
            shouldBeLowerCase[i] = (shouldBePrefixedWithSpace[i] && NotNextUpper(i, value));

        var sb = new StringBuilder();

        for (int i = 0; i < value.Length; i++)
        {
            bool isJustFine = !IsFirst(i) && !shouldBeLowerCase[i];
            if (shouldBePrefixedWithSpace[i]) AppendSpace(sb);
            if (IsFirst(i)) AppendCharInUpperCase(sb, value[i]);
            if (shouldBeLowerCase[i]) AppendCharInLowerCase(sb, value[i]);
            if (isJustFine) AppendCharAsIs(sb, value[i]);
        }

        return sb.ToString();
    }

    public static string ConversionWithoutHumanizerLongVersion(string value)
    {
        bool[] shouldBePrefixedWithSpace = new bool[value.Length];
        bool[] shouldBeLowerCase = new bool[value.Length];

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isNeedSpaceForPascalCase
                = IsUpper(c)
                && NotPreviousUpper(i, value)
                && NotFirst(i);

            bool isNeedSpaceForAcronym
                = IsUpper(c)
                && IsPreviousUpper(i, value)
                && IsSecondPreviousUpper(i, value)
                && NotNextUpper(i, value)
                && NotLast(i, value);

            shouldBePrefixedWithSpace[i] = (isNeedSpaceForPascalCase || isNeedSpaceForAcronym);
            shouldBeLowerCase[i] = (shouldBePrefixedWithSpace[i] && NotNextUpper(i, value));
        }

        var sb = new StringBuilder();

        for (int i = 0; i < value.Length; i++)
        {
            bool isJustFine = !IsFirst(i) && !shouldBeLowerCase[i];
            if (shouldBePrefixedWithSpace[i]) AppendSpace(sb);
            if (IsFirst(i)) AppendCharInUpperCase(sb, value[i]);
            if (shouldBeLowerCase[i]) AppendCharInLowerCase(sb, value[i]);
            if (isJustFine) AppendCharAsIs(sb, value[i]);
        }

        return sb.ToString();
    }
}