using System.Reflection;

namespace MikeyT.DbMigrations;

public static class TypeHelper
{
    /// <summary>
    /// Determine if a class is in an assembly. Case sensitive. Class name only and fully qualified type name is allowed.
    /// </summary>
    /// <param name="className">The class name or full name. Case sensitive.</param>
    /// <param name="assembly">The assembly to check for the class.</param>
    /// <returns><c>True</c> if the class is in the assembly, <c>False</c> otherwise</returns>
    public static bool DoesClassExist(string className, Assembly assembly)
    {
        var classNameTrimmed = ValidateParamsAndReturnTrimmedClassName(className, assembly);
        Type[] allTypes = assembly.GetTypes();
        foreach (Type type in allTypes)
        {
            if (type.Name == classNameTrimmed || type.FullName == classNameTrimmed)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Determine if an assembly has exactly one class with the specified name. Case sensitive. Class name only and fully qualified type name is allowed.
    /// </summary>
    /// <param name="className">The class to search for.</param>
    /// <param name="assembly">The assembly to search in.</param>
    /// <returns><c>True</c> if exactly one class in the assembly matches the name passed, <c>False otherwise</c></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool DoesExactlyOneClassExist(string className, Assembly assembly)
    {
        var classNameTrimmed = ValidateParamsAndReturnTrimmedClassName(className, assembly);
        return GetTypeIfExactlyOneInternal(classNameTrimmed, assembly) != null;
    }

    /// <summary>
    /// Get a type from an assembly if exactly one type matches the name passed. Case sensitive. Class name only and fully qualified type name is allowed.
    /// </summary>
    /// <param name="className">The class to search for.</param>
    /// <param name="assembly">The assembly to search in.</param>
    /// <returns>The type if exactly one type in the assembly matches the name passed, <c>Null</c> otherwise</returns>
    public static Type? GetTypeIfExactlyOne(string className, Assembly assembly)
    {
        var classNameTrimmed = ValidateParamsAndReturnTrimmedClassName(className, assembly);
        return GetTypeIfExactlyOneInternal(classNameTrimmed, assembly);
    }

    /// <summary>
    /// Determine if a type implements an interface.
    /// </summary>
    /// <param name="typeToCheck">The type to check.</param>
    /// <param name="interfaceType">The interface type to check for.</param>
    /// <returns><c>True</c> if the type implements the interface, <c>False</c> otherwise</returns>
    public static bool TypeImplementsInterface(Type typeToCheck, Type interfaceType)
    {
        if (interfaceType.IsGenericTypeDefinition)
        {
            return typeToCheck.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
        else
        {
            return interfaceType.IsAssignableFrom(typeToCheck);
        }
    }

    public static Type? GetGenericInterfaceType(Type targetType, Type genericInterfaceType)
    {
        if (!genericInterfaceType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The provided interface type must be a generic type definition");
        }

        var interfaceType = targetType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);

        return interfaceType?.GetGenericArguments().FirstOrDefault();
    }

    private static Type? GetTypeIfExactlyOneInternal(string className, Assembly assembly)
    {
        Type[] allTypes = assembly.GetTypes();
        Type? match = null;
        foreach (Type type in allTypes)
        {
            if (type.Name == className || type.FullName == className)
            {
                if (match != null)
                {
                    return null;
                }
                match = type;
            }
        }
        return match;
    }

    private static string ValidateParamsAndReturnTrimmedClassName(string className, Assembly assembly)
    {
        if (className == null || className.Trim() == string.Empty)
        {
            throw new ArgumentException("missing required param", nameof(className));
        }

        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        return className.Trim();
    }
}
