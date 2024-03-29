﻿namespace Brimborium.Registrator;

internal static class ReflectionExtensions {
    public static bool DefaultPredicateAssemblyName(AssemblyName assemlbyName) {
        if (assemlbyName.Name is null) {
            return false;
        }

        // assume that this is not used by a system library...
        if (assemlbyName.Name.Equals("System", StringComparison.Ordinal)) { return false; }
        if (assemlbyName.Name.Equals("mscorlib", StringComparison.Ordinal)) { return false; }
        if (assemlbyName.Name.Equals("netstandard", StringComparison.Ordinal)) { return false; }
        if (assemlbyName.Name.Equals("WindowsBase", StringComparison.Ordinal)) { return false; }
        if (assemlbyName.Name.StartsWith("System.", StringComparison.Ordinal)) { return false; }

        // MSBuild Locator does not liked to be waked up too early - keep the fingers away from it.
        if (assemlbyName.Name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal)) { return false; }

        // speed
        if (assemlbyName.Name.StartsWith("Microsoft.", StringComparison.Ordinal)) { return false; }
        if (assemlbyName.Name.Equals("Brimborium.Registrator", StringComparison.Ordinal)) { return false; }
        if (assemlbyName.Name.Equals("Brimborium.Registrator.Abstractions", StringComparison.Ordinal)) { return false; }

        return true;
    }

    public static bool IsNonAbstractClass(this Type type, bool publicOnly) {
        var typeInfo = type.GetTypeInfo();

        if (typeInfo.IsSpecialName) {
            return false;
        }

        if (typeInfo.IsClass && !typeInfo.IsAbstract) {
            if (typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true)) {
                return false;
            }

            if (publicOnly) {
                return typeInfo.IsPublic || typeInfo.IsNestedPublic;
            }

            return true;
        }

        return false;
    }

    public static IEnumerable<Type> GetBaseTypes(
        this Type type,
        bool includeInterfaces,
        bool includeClasses
        ) {

        if (includeInterfaces) {
            foreach (var implementedInterface in type.GetTypeInfo().ImplementedInterfaces) {
                if (implementedInterface.HasMatchingGenericArity(type.GetTypeInfo())) {
                    yield return implementedInterface;
                }
            }
        }

        if (includeClasses) {
            var baseType = type.GetTypeInfo().BaseType;

            while (baseType != null) {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }

    public static bool IsInNamespace(this Type type, string @namespace) {
        var typeNamespace = type.Namespace ?? string.Empty;

        if (@namespace.Length > typeNamespace.Length) {
            return false;
        }

        var typeSubNamespace = typeNamespace.Substring(0, @namespace.Length);

        if (typeSubNamespace.Equals(@namespace, StringComparison.Ordinal)) {
            if (typeNamespace.Length == @namespace.Length) {
                //exactly the same
                return true;
            }

            //is a subnamespace?
            return typeNamespace[@namespace.Length] == '.';
        }

        return false;
    }

    public static bool IsInExactNamespace(this Type type, string @namespace) {
        return string.Equals(type.Namespace, @namespace, StringComparison.Ordinal);
    }

    public static bool HasAttribute(this Type type, Type attributeType) {
        return type.GetTypeInfo().IsDefined(attributeType, inherit: true);
    }

    public static bool HasAttribute<T>(this Type type, Func<T, bool> predicate) where T : Attribute {
        return type.GetTypeInfo().GetCustomAttributes<T>(inherit: true).Any(predicate);
    }

    public static bool IsAssignableTo(this Type type, Type otherType) {
        var typeInfo = type.GetTypeInfo();
        var otherTypeInfo = otherType.GetTypeInfo();

        if (otherTypeInfo.IsGenericTypeDefinition) {
            return typeInfo.IsAssignableToGenericTypeDefinition(otherTypeInfo);
        }

        return otherTypeInfo.IsAssignableFrom(typeInfo);
    }

    private static bool IsAssignableToGenericTypeDefinition(this TypeInfo typeInfo, TypeInfo genericTypeInfo) {
        var interfaceTypes = typeInfo.ImplementedInterfaces.Select(t => t.GetTypeInfo());

        foreach (var interfaceType in interfaceTypes) {
            if (interfaceType.IsGenericType) {
                var typeDefinitionTypeInfo = interfaceType
                    .GetGenericTypeDefinition()
                    .GetTypeInfo();

                if (typeDefinitionTypeInfo.Equals(genericTypeInfo)) {
                    return true;
                }
            }
        }

        if (typeInfo.IsGenericType) {
            var typeDefinitionTypeInfo = typeInfo
                .GetGenericTypeDefinition()
                .GetTypeInfo();

            if (typeDefinitionTypeInfo.Equals(genericTypeInfo)) {
                return true;
            }
        }

        var baseTypeInfo = typeInfo.BaseType?.GetTypeInfo();

        if (baseTypeInfo is null) {
            return false;
        }

        return baseTypeInfo.IsAssignableToGenericTypeDefinition(genericTypeInfo);
    }

    /// <summary>
    /// Find matching interface by name C# interface name convention.  Optionally use a filter.
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<Type> FindMatchingInterface(this TypeInfo typeInfo, Action<TypeInfo, IImplementationTypeFilter>? action) {
        var matchingInterfaceName = $"I{typeInfo.Name}";

        var matchedInterfaces = GetImplementedInterfacesToMap(typeInfo)
            .Where(x => string.Equals(x.Name, matchingInterfaceName, StringComparison.Ordinal))
            .ToArray();

        Type? type = null;
        if (action is null) {
            type = matchedInterfaces.FirstOrDefault();
        } else {
            var filter = new ImplementationTypeFilter(matchedInterfaces);

            action(typeInfo, filter);

            type = filter.Types.FirstOrDefault();
        }

        if (type is null) {
            yield break;
        }

        yield return type;
    }

    private static IEnumerable<Type> GetImplementedInterfacesToMap(TypeInfo typeInfo) {
        if (!typeInfo.IsGenericType) {
            return typeInfo.ImplementedInterfaces;
        }

        if (!typeInfo.IsGenericTypeDefinition) {
            return typeInfo.ImplementedInterfaces;
        }

        return FilterMatchingGenericInterfaces(typeInfo);
    }

    private static IEnumerable<Type> FilterMatchingGenericInterfaces(TypeInfo typeInfo) {
        var genericTypeParameters = typeInfo.GenericTypeParameters;

        foreach (var current in typeInfo.ImplementedInterfaces) {
            var currentTypeInfo = current.GetTypeInfo();

            if (currentTypeInfo.IsGenericType && currentTypeInfo.ContainsGenericParameters
                && GenericParametersMatch(genericTypeParameters, currentTypeInfo.GenericTypeArguments)) {
                yield return currentTypeInfo.GetGenericTypeDefinition();
            }
        }
    }

    private static bool GenericParametersMatch(IReadOnlyList<Type> parameters, IReadOnlyList<Type> interfaceArguments) {
        if (parameters.Count != interfaceArguments.Count) {
            return false;
        }

        for (var i = 0; i < parameters.Count; i++) {
            if (parameters[i] != interfaceArguments[i]) {
                return false;
            }
        }

        return true;
    }

    public static string ToFriendlyName(this Type type) {
        return TypeNameHelper.GetTypeDisplayName(type, includeGenericParameterNames: true);
    }

    public static bool IsOpenGeneric(this Type type) {
        return type.GetTypeInfo().IsGenericTypeDefinition;
    }

    public static bool HasMatchingGenericArity(this Type interfaceType, TypeInfo typeInfo) {
        if (typeInfo.IsGenericType) {
            var interfaceTypeInfo = interfaceType.GetTypeInfo();

            if (interfaceTypeInfo.IsGenericType) {
                var argumentCount = interfaceType.GenericTypeArguments.Length;
                var parameterCount = typeInfo.GenericTypeParameters.Length;

                return argumentCount == parameterCount;
            }

            return false;
        }

        return true;
    }

    public static Type GetRegistrationType(this Type interfaceType, TypeInfo typeInfo) {
        if (typeInfo.IsGenericTypeDefinition) {
            var interfaceTypeInfo = interfaceType.GetTypeInfo();

            if (interfaceTypeInfo.IsGenericType) {
                return interfaceType.GetGenericTypeDefinition();
            }
        }

        return interfaceType;
    }
}
