using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// Represents a parameter for <see cref="FunctionMethodInfo"/>.
/// </summary>
public class FunctionParameterInfo
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public bool IsNullable { get; set; }
    public object? Value { get; set; }
}
