using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// Dynamically use reflection to invoke a method from a class.
/// Function selection is based on the json string passed in.
/// </summary>
public class FunctionInvoker
{
    /// <summary>
    /// Invokes the methods from the JSON string.
    /// </summary>
    /// <param name="json">json from LLM representing the function to invoke</param>
    /// <param name="functionMethodsInfo">List of function available to invoke.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public object? InvokeMethodsFromJson(string json, List<FunctionMethodInfo> functionMethodsInfo)
    {
        // Deserialize the JSON to get the function calls
        var functionCall = ConvertJsonToFunctionMethodInfo(json);
        var classType = FindMethod(functionCall, functionMethodsInfo);

        if (classType != null)
        {
            var methodInfo = classType.GetMethod(functionCall.Name!);

            if (methodInfo != null)
            {
                var parameters = GetParameters(methodInfo, functionCall);

                // Instantiate the class
                var targetInstance = Activator.CreateInstance(classType);
                if (targetInstance == null)
                {
                    throw new InvalidOperationException($"Could not instantiate type {classType.FullName}");
                }

                // Invoke the method
                var result = methodInfo.Invoke(targetInstance, parameters.ToArray());

                return result;
            }
        }

        return null;
    }

    private Type? FindMethod(FunctionMethodInfo functionCall, List<FunctionMethodInfo> functionMethodsInfo)
    {
        return (from functionMethodInfo in functionMethodsInfo where functionMethodInfo.Name == functionCall.Name select functionMethodInfo.ClassType).FirstOrDefault();
    }

    private static List<object> GetParameters(MethodInfo methodInfo, FunctionMethodInfo functionCall)
    {
        // Prepare the parameters for the method call
        var parameters = new List<object>();
        foreach (var param in methodInfo.GetParameters())
        {
            // Find the corresponding parameter in the list
            var functionParam = functionCall.Parameters?.FirstOrDefault(p => p.Name == param.Name);
            if (functionParam != null)
            {
                // Convert the parameter value to the correct type
                parameters.Add(Convert.ChangeType(functionParam.Value, param.ParameterType)!);
            }
            else
            {
                throw new ArgumentException($"Parameter {param.Name} not found for method {functionCall.Name}");
            }
        }

        return parameters;
    }

    private static FunctionMethodInfo ConvertJsonToFunctionMethodInfo(string json)
    {
        var tempFunctionCall = JsonConvert.DeserializeObject<TempFunctionCall>(json);

        var functionMethodInfo = new FunctionMethodInfo
        {
            Name = tempFunctionCall?.Name,
            Parameters = new List<FunctionParameterInfo>()
        };

        if (tempFunctionCall?.Parameters != null)
        {
            foreach (var param in tempFunctionCall.Parameters)
            {
                functionMethodInfo.Parameters.Add(new FunctionParameterInfo { Name = param.Key, Value = param.Value });
            }
        }

        return functionMethodInfo;
    }
}
