using System;

/// <summary>
/// The FunctionFinder class is responsible for finding all methods in the current assembly that are decorated with the FunctionAttribute.
/// </summary>
public static class FunctionFinder
{
    /// <summary>
    /// Finds all methods in the current assembly that are decorated with the FunctionAttribute and returns them as an indented JSON string.
    /// </summary>
    /// <returns>indented JSON string that will ignore non-resolved properties.</returns>
    public static string FindFunctionAttributedMethodsJson()
    {
        return FindFunctionAttributedMethodsJson(GetFunctionAttributedMethodsInfo());
    }

    /// <summary>
    /// Finds all methods in the current assembly that are decorated with the FunctionAttribute and returns them as an indented JSON string.
    /// </summary>
    /// <returns>indented JSON string that will ignore non-resolved properties.</returns>
    public static string FindFunctionAttributedMethodsJson(List<FunctionMethodInfo> functionMethodInfos)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreIntentPropertyResolver(),
            Formatting = Formatting.Indented
        };

        return JsonConvert.SerializeObject(functionMethodInfos, settings);
    }

    /// <summary>
    /// Finds all methods in the current assembly that are decorated with the FunctionAttribute and returns them as a list of FunctionMethodInfo objects.
    /// </summary>
    /// <returns></returns>
    public static List<FunctionMethodInfo> GetFunctionAttributedMethodsInfo()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        return GetFunctionAttributedMethodsInfoFromAssembly(currentAssembly);
    }

    /// <summary>
    /// Finds all methods in the provided assembly that are decorated with the FunctionAttribute and returns them as a list of FunctionMethodInfo objects.
    /// </summary>
    /// <param name="currentAssembly"></param>
    /// <returns></returns>
    public static List<FunctionMethodInfo> GetFunctionAttributedMethodsInfoFromAssembly(Assembly currentAssembly)
    {
        var attributedMethodsInfo = currentAssembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Where(method => method.GetCustomAttributes<FunctionAttribute>(inherit: true).Any() &&
                             method.GetCustomAttributes<DescriptionAttribute>(inherit: true).Any())
            .Select(method => new FunctionMethodInfo
            {
                Name = method.Name,
                Intent = method.GetCustomAttribute<FunctionAttribute>()?.Intent,
                Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description,
                Parameters = method.GetParameters().Select(param => new FunctionParameterInfo
                {
                    Name = param.Name,
                    Type = param.ParameterType.ToString(),
                    IsNullable = IsParameterNullable(param)
                }).ToList(),
                Required = method.GetParameters()
                    .Where(param => !IsParameterNullable(param))
                    .Select(param => param.Name)
                    .ToList()!,
                ClassType = method.DeclaringType // Populate the ClassType property with the declaring type of the method
            }).ToList();

        return attributedMethodsInfo;
    }

    private static bool IsParameterNullable(ParameterInfo parameter)
    {
        // Check for [NotNull] attribute first
        if (parameter.GetCustomAttributes<NotNullAttribute>().Any())
        {
            return false;
        }

        // Check if the parameter has a default value of null
        if (parameter is { HasDefaultValue: true, DefaultValue: null })
        {
            return true;
        }

        // For C# 8.0 nullable reference types, check the nullability context
        var nullability = parameter.CustomAttributes
            .FirstOrDefault(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute")?
            .ConstructorArguments.FirstOrDefault().Value;

        return nullability switch
        {
            byte[] typeArray => typeArray[0] == 2,
            byte typeValue => typeValue == 2,
            // Fallback to treating reference types as nullable and value types based on whether they're nullable value types
            _ => Nullable.GetUnderlyingType(parameter.ParameterType) != null || !parameter.ParameterType.IsValueType
        };
    }
}
