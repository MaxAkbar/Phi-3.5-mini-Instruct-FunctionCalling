using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// A decorator attribute that marks a method as a function to be used in a Large Language Model.
/// </summary>
public class FunctionAttribute : Attribute
{
    /// <summary>Initializes the attribute.</summary>
    /// <param name="intent">A simple sentence to describe the intent of a user question to the function.</param>
    /// <param name="send"></param>
    public FunctionAttribute(string intent, bool send)
    {
        Intent = intent;
        Send = send;
    }

    /// <summary>Gets the function's intent.</summary>
    public string Intent { get; }

    public bool Send { get; }
}
