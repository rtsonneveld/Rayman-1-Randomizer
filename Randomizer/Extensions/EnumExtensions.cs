using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rayman1Randomizer;

/// <summary>
/// Extension methods for <see cref="Enum"/>
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the first attribute of specified type for the enum value
    /// </summary>
    /// <typeparam name="T">The type of attribute to retrieve</typeparam>
    /// <param name="value">The enum value to get the attribute for</param>
    /// <returns>The attribute instance</returns>
    public static T? GetAttribute<T>(this Enum? value) 
        where T : Attribute
    {
        if (value == null)
            return null;

        // Get the member info for the value
        MemberInfo[] memberInfo = value.GetType().GetMember(value.ToString());

        // Get the attribute
        IEnumerable<T>? attributes = memberInfo.FirstOrDefault()?.GetCustomAttributes<T>(false);

        // Return the first attribute
        return attributes?.FirstOrDefault<T>();
    }
}