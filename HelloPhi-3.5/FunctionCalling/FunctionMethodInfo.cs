using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// Represents a method that is a function to be used.
/// </summary>
public class FunctionMethodInfo
{
    public string? Name { get; set; }
    public string? Intent { get; set; }
    public string? Description { get; set; }
    public List<FunctionParameterInfo>? Parameters { get; set; }
    public List<string>? Required { get; set; }
    public Type? ClassType { get; set; }
}
