namespace Brimborium.Registrator;

/// <summary>
/// Using an IOptionsMonitor to provide a snapshot of the current value of the options.
/// This is normally a singelton that also uses OnChange to update the current value.
/// </summary>
/// <typeparam name="TOptions">The options type</typeparam>
public interface IOptionsCurrent<TOptions>
    : IDisposable
    where TOptions : class {
    /// <summary>
    /// The current latest value.
    /// </summary>
    TOptions CurrentValue { get; }

    /// <summary>
    /// a incremental version
    /// </summary>
    long Version { get; }

    /// <summary>
    /// Get the current value and version.
    /// </summary>
    (long version, TOptions currentValue) GetCurrent();

    /// <summary>
    /// Create a new OtionsValue
    /// </summary>
    IOptionsValue<TOptions> GetOptionsValue();

    /// <summary>
    /// a new IOptionsCurrent using the function projectValue to create a new derived value.
    /// </summary>
    /// <typeparam name="TOptionsTarget"></typeparam>
    /// <param name="projectValue">a function that project the value</param>
    /// <returns>a new instance</returns>
    IOptionsCurrent<TOptionsTarget> GetOptionsCurrentProjected<TOptionsTarget>(Func<TOptions, TOptionsTarget> projectValue)
         where TOptionsTarget : class;
}

/// <summary>
/// Using a IOptionsCurrent to provide a snapshot of the current value of the options.
/// You can control if you want the current value or the not updated value.
/// </summary>
/// <typeparam name="TOptions">The options type</typeparam>
public interface IOptionsValue<TOptions> where TOptions : class {
    /// <summary>
    /// The current latest value.
    /// </summary>
    TOptions CurrentValue { get; }

    /// <summary>
    /// The value of initalization or of the last time 'you' called CurrentValue.
    /// </summary>
    TOptions Value { get; }

    /// <summary>
    /// a incremental version
    /// </summary>
    long Version { get; }
}
