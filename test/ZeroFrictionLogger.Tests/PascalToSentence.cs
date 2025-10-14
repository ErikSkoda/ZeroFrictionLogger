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
//
// This method implements PascalCase to sentence conversion,
// inspired by similar functionality found in the Humanizer package.
// Written entirely from scratch without using or viewing their source code.

using System.Text;
using System.Linq; // required to make lib #legacy #compliant with .Net Core 2.1 (Out of support)

namespace ZeroFrictionLogger // #compliant with both #legacy .Net Core 2.1 and .Net Core 8.0 LTS
{
    internal static class PascalToSentence // eliminates #dependency on Humanizer package
    {
        private const int _firstCharacterIndex = 0;
        private const int _secondCharacterIndex = 1;
        private const int _lastCharacterOffset = -1;
        private const int _previousCharacterOffset = 1;
        private const int _nextCharacterOffset = 1;
        private const int _secondPreviousCharacterOffset = 2;
        private const char _spaceChar = ' ';

        // #logic lives below here, sort of like a formularium in insurance industry.
        private static bool IsLogicalToAddSpaceForPascalCase(int i, string value)
            => IsUpperCase(value[i])
            && NotPreviousUpperCase(i, value)
            && NotFirstCharacter(i);

        private static bool IsLogicalToAddSpaceForAcronym(int i, string value)
            => IsUpperCase(value[i])
            && IsPreviousUpperCase(i, value)
            && IsSecondPreviousUpperCase(i, value)
            && NotNextUpperCase(i, value)
            && NotLastCharacter(i, value);

        private static bool IsLogicalToAddSpace(int i, string value)
            => IsLogicalToAddSpaceForPascalCase(i, value)
            || IsLogicalToAddSpaceForAcronym(i, value);

        private static bool IsExclusiveLogicUpperCase(int i) => IsFirstCharacter(i);

        private static bool IsExclusiveLogicLowerCase(int i, string valueWithSpacesInserted)
            => IsUpperCase(valueWithSpacesInserted[i])
            && IsNextLowerCase(i, valueWithSpacesInserted)
            && IsPreviousCharSpace(i, valueWithSpacesInserted)
            && !IsExclusiveLogicUpperCase(i); // << explicit exclusive (optional, works without)

        private static bool IsExclusiveLogicLeaveUnchanged(int i, string valueWithSpacesInserted)
            => !IsExclusiveLogicUpperCase(i) // << explicit exclusive (must have)
            && !IsExclusiveLogicLowerCase(i, valueWithSpacesInserted); // << explicit (must have)

        // #helpers for logic readability below here.
        private static bool IsFirstCharacter(int i) => i == _firstCharacterIndex;
        private static bool NotFirstCharacter(int i) => !IsFirstCharacter(i);
        private static bool IsSecondCharacter(int i) => i == _secondCharacterIndex;
        private static bool IsLastCharacter(int i, string value)
            => i == value.Length + _lastCharacterOffset;
        private static bool NotLastCharacter(int i, string value) => !IsLastCharacter(i, value);
        private static bool IsUpperCase(char c) => char.IsUpper(c);
        private static bool IsPreviousUpperCase(int i, string value)
            => !IsFirstCharacter(i) && char.IsUpper(value[i - _previousCharacterOffset]);
        private static bool NotPreviousUpperCase(int i, string value)
            => !IsPreviousUpperCase(i, value);
        private static bool IsPreviousCharSpace(int i, string value)
            => !IsFirstCharacter(i) && (value[i - _previousCharacterOffset] == _spaceChar);
        private static bool IsSecondPreviousUpperCase(int i, string value)
            => !IsFirstCharacter(i)
                && !IsSecondCharacter(i)
                && char.IsUpper(value[i - _secondPreviousCharacterOffset]);
        private static bool IsNextUpper(int i, string value)
            => NotLastCharacter(i, value) && char.IsUpper(value[i + _nextCharacterOffset]);
        private static bool IsNextLowerCase(int i, string value)
            => NotLastCharacter(i, value) && char.IsLower(value[i + _nextCharacterOffset]);
        private static bool NotNextUpperCase(int i, string value) => !IsNextUpper(i, value);

        private static bool[] GetNeedForSpace(string value) =>
             value.Select((_, i) => IsLogicalToAddSpace(i, value)).ToArray();

        // #application of logic below here
        public static string ApplyLogicForSpacing(string value)
        {
            var sb = new StringBuilder();
            bool[] shouldIncludeSpace = GetNeedForSpace(value);
            bool[] shouldNotIncludeSpace = shouldIncludeSpace.Select(yes => !yes).ToArray();

            for (int i = 0; i < value.Length; i++)
            {
                if (shouldIncludeSpace[i]) AppendCharWithSpacePrefix(sb, value[i]);
                if (shouldNotIncludeSpace[i]) AppendCharAsIs(sb, value[i]);
            }   // ^^ enforced mutually exclusive if statements, replacing an else
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

        private static void AppendCharInLowerCase(StringBuilder sb, char value)
            => sb.Append(value.ToString().ToLowerInvariant());
        private static void AppendCharInUpperCase(StringBuilder sb, char value)
            => sb.Append(value.ToString().ToUpperInvariant());
        //private static void AppendUnderScoreForDebugging(StringBuilder sb)
        //    => sb.Append('_'); // #debug
        private static void AppendCharAsIs(StringBuilder sb, char value) => sb.Append(value);
        private static void AppendCharWithSpacePrefix(StringBuilder sb, char value)
        {
            sb.Append(_spaceChar);
            sb.Append(value);
        }

        private static string ConvertPascalCaseToSentenceWithoutHumanizer(string value)
        {
            string spaced = ApplyLogicForSpacing(value);
            return ApplyLogicForCasingAfterSpacing(spaced);
        }

        public static string ConvertPascalCaseToSentence(string value)
        {
            try
            {
                return ConvertPascalCaseToSentenceWithoutHumanizer(value);
            }
            catch
            {
                return value; // fallback to passed param in case of an exception.
            }
        }
    }
}
