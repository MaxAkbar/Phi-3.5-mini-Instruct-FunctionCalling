using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// FunctionExtractor class to extract function calls from a given input.
/// Input is expected to be in the format:
/// <![CDATA[
/// <|thoughts|>
/// The user is asking for the result of a simple addition operation.Since the Add function is available, I will use it to calculate the sum of 2 and 2.
/// <|function_calls|>
/// [
/// { "name": "Add", "parameters": {"num1": 2, "num2": 2}, "returns": ["4"]},
/// <| end_function_calls |>
/// <| end_thoughts |>
/// ]]>
/// The return is expected to be in the format:
/// <![CDATA[
/// { "name": "Add", "parameters": {"num1": 2, "num2": 2}, "returns": ["4"]}
/// ]]>
/// </summary>
public class FunctionExtractor
{
    private static readonly Regex HasFunctionCallRegex = new(@"<\|function_calls\|>[\s\S]*<\|end_function_calls\|>", RegexOptions.Compiled);
    private static readonly Regex ExtractFunctionCallRegex = new(@"(?<=<\|function_calls\|>(\s*)\[\s*){\w*.*}", RegexOptions.Compiled);

    /// <summary>
    /// Extract the function call from the given input.
    /// </summary>
    /// <param name="input">string input having the function call embedded.</param>
    /// <returns></returns>
    public static string? ExtractFunctionCall(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // given the input find out if we have any function calls
        (bool Success, string Match) functionCall = HasFunctionCalls(input);

        if (!functionCall.Success)
        {
            return null;
        }

        functionCall = ExtractFunctionCalls(functionCall.Match);

        return functionCall.Success ? functionCall.Match : null;
    }

    private static (bool, string) HasFunctionCalls(string input)
    {
        var match = HasFunctionCallRegex.Match(input);

        return (match.Success, match.Groups[0].Value);
    }

    private static (bool, string) ExtractFunctionCalls(string input)
    {
        var match = ExtractFunctionCallRegex.Match(input);

        return (match.Success, match.Groups[0].Value);
    }
}
