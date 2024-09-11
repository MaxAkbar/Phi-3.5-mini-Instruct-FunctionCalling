using HelloPhi35.FunctionCalling;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

//var modelPath = "models\\Phi-3.5-cpu-onnx\\";
var modelPath = "models\\onnx-cuda-fp32\\";
var continueChat = true;

Console.WriteLine("Loading model...");

using OgaHandle ogaHandle = new OgaHandle();
using Model model = new Model(modelPath);
using Tokenizer tokenizer = new Tokenizer(model);

Console.WriteLine("Hello, you have rached the customer service refunding system. To refund an order please provide your email address.");
Console.WriteLine("Type '/exit' to end the conversation.");

do
{
    Console.Write("User: ");
    var userQ = Console.ReadLine();

    if (string.IsNullOrEmpty(userQ))
    {
        continue;
    }

    if (userQ == "/exit")
    {
        continueChat = false;
        break;
    }

    using GeneratorParams generatorParams = new(model);

    var intentResponse = GetUserIntent(userQ, generatorParams);
    var functionResult = FunctionCall(intentResponse, userQ, generatorParams);

    if (string.IsNullOrEmpty(intentResponse) || functionResult != null)
    {
        Console.WriteLine(functionResult);
    }
} while (continueChat);

string GetUserIntent(string userQ, GeneratorParams generatorParams)
{
    var intentPrompt = GetIntentPrompt(userQ);

    if (string.IsNullOrEmpty(intentPrompt)) return string.Empty;

    var sequences = tokenizer.Encode($"<|user|>{intentPrompt}<|end|><|assistant|>");

    var intentResponse = GetPhi35Response(sequences, generatorParams);

    return intentResponse;
}

IEnumerable<string?> GetPhi35StreamedResponse(Sequences prompt, GeneratorParams generatorParams, bool debugInfo = false)
{
    using var tokenizerStream = tokenizer.CreateStream();
    using var generator = new Generator(model, generatorParams);
    var watch = System.Diagnostics.Stopwatch.StartNew();

    if (debugInfo)
    {
        Console.Write("Assistant: ");
    }

    while (!generator.IsDone())
    {
        generator.ComputeLogits();
        generator.GenerateNextToken();

        yield return tokenizerStream.Decode(generator.GetSequence(0)[^1]);
    }

    if (debugInfo)
    {
        watch.Stop();
        var runTimeInSeconds = watch.Elapsed.TotalSeconds;
        var outputSequence = generator.GetSequence(0);
        var totalTokens = outputSequence.Length;

        Console.WriteLine();
        Console.WriteLine($"Streaming Tokens: {totalTokens} Time: {runTimeInSeconds:0.00} Tokens per second: {totalTokens / runTimeInSeconds:0.00}");
    }
}

string GetPhi35Response(Sequences prompt, GeneratorParams generatorParams, bool debugInfo = false)
{
    generatorParams.SetSearchOption("max_length", 4096);
    generatorParams.SetInputSequences(prompt);

    var sb = new StringBuilder();

    foreach (var response in GetPhi35StreamedResponse(prompt, generatorParams, debugInfo))
    {
        sb.Append(response);
    }

    return sb.ToString();
}

string GetIntentPrompt(string userQ)
{
    // first get the functions intents
    var functionAttributedMethodsInfo = FunctionFinder.GetFunctionAttributedMethodsInfo();

    if (functionAttributedMethodsInfo.Count <= 0) return string.Empty;

    var intents = new StringBuilder();
    var intentPrompt = File.ReadAllText(Path.Combine("Prompts", "IntentPrompt.txt"));

    // get the function prompt
    foreach (var functionMethodInfo in functionAttributedMethodsInfo)
    {
        intents.AppendLine(functionMethodInfo.Intent);
    }

    intentPrompt = intentPrompt.
        Replace("{QUESTION}", userQ).
        Replace("{INTENTS}", intents.ToString().TrimEnd());

    return intentPrompt;
}

object? FunctionCall(string function, string message, GeneratorParams generatorParams)
{
    if (message.ToLower().Trim().Contains("no intent found for the question."))
        return null;

    // get the function prompt
    var functionPrompt = LoadFunctionPrompts(function);

    // add the user question to the function prompt by using string replacement
    if (string.IsNullOrEmpty(functionPrompt)) return null;

    // get the intent from the chatHistory user's question
    var fullPrompt = functionPrompt.Replace("{Question}", message);
    var sequences = tokenizer.Encode(fullPrompt);
    var phi3Response = GetPhi35Response(sequences, generatorParams);

    if (string.IsNullOrEmpty(phi3Response)) return null;

    // extract the function from the response
    var extractFunctionCall = ExtractFunctionCall(phi3Response);

    if (string.IsNullOrEmpty(extractFunctionCall)) return null;

    var executingAssembly = Assembly.GetExecutingAssembly();
    var functionsAttributedMethodsInfo = FunctionFinder.GetFunctionAttributedMethodsInfoFromAssembly(executingAssembly);
    var reflectionInvoker = new FunctionInvoker();
    var functionResult = reflectionInvoker.InvokeMethodsFromJson(extractFunctionCall, functionsAttributedMethodsInfo);

    return functionResult;
}

string LoadFunctionPrompts(string function)
{
    var functions = FunctionFinder.FindFunctionAttributedMethodsJson();
    var functionPrompt = File.ReadAllText(@"Prompts\FunctionPrompt.txt");

    return functionPrompt.Replace("{Functions}", functions);
}

static string? ExtractFunctionCall(string input)
{
    var pattern = @"(?<=<\|function_calls\|>(\s*\[)?)?\s*\{*[\s*|\r|\n]\s*\""name\"":\s*""[^""]*""\s*,(\s*[^}]*\s*[\""parameters\"":\s*\{[^}]*,?[^}])?(\s*\""output\"":\s*""[^""]*""\s*\})?\s*(?=\]|<\|end_function_calls\|>)?";
    var match = Regex.Match(input, pattern, RegexOptions.Singleline);

    if (match.Success)
    {
        // Return the matched group which contains the function call
        return match.Groups[0].Value.Trim();
    }

    return null;
}