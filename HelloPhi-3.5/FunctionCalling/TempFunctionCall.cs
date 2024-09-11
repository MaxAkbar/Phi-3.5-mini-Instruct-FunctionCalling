using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// Represents a temporary function parameters.
/// We do this as the SLM returns a dictionary of parameters but our function expects a List.
/// </summary>
public class TempFunctionCall
{
    public string? Name { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public string? Output { get; set; }
}
