﻿namespace Brimborium.Registrator;

/// <summary>
/// Annotate class as options.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class OptionsAttribute : Attribute {
    /// <summary>
    /// Create new instance, section mode used to bind application settings property to annotated class.
    /// </summary>
    /// <param name="section"></param>
    public OptionsAttribute(string? section = default) {
        Section = section ?? string.Empty;
    }

    /// <summary>
    /// Section name
    /// </summary>
    public string Section { get; }
}
