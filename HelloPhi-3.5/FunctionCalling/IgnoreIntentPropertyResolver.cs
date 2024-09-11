using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling;

/// <summary>
/// A custom contract resolver that ignores the Intent property of a FunctionMethodInfo object when serializing to JSON.
/// </summary>
public class IgnoreIntentPropertyResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.DeclaringType == typeof(FunctionMethodInfo) && property.PropertyName == "ClassType")
        {
            property.ShouldSerialize = _ => false;
        }

        return property;
    }
}
