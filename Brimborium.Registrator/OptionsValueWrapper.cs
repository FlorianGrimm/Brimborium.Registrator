namespace Brimborium.Registrator;

/// <summary>
/// Helper to create IOptionsCurrent and IOptionsValue for unit-testing.
/// </summary>
public static class OptionsValueWrapper{
    /// <summary>
    /// Create an <see cref="IOptionsCurrent{TOptions}"/> for unit-testing.
    /// </summary>
    /// <typeparam name="TOptions">The options type (class)</typeparam>
    /// <param name="option">The option instance.</param>
    /// <returns>an new instance of <see cref="IOptionsCurrent{TOptions}"/>.</returns>
    public static IOptionsCurrent<TOptions> CreateOptionsCurrent<TOptions>(TOptions option) 
        where TOptions : class 
        => new OptionsCurrent<TOptions>(new OptionsMonitorWrapper<TOptions>(option));

    /// <summary>
    /// Create an <see cref="IOptionsCurrent{TOptions}"/> for unit-testing.
    /// </summary>
    /// <typeparam name="TOptions">The options type (class)</typeparam>
    /// <param name="optionsMonitor">The source of the option</param>
    /// <returns>an new instance of <see cref="IOptionsCurrent{TOptions}"/>.</returns>
    public static IOptionsCurrent<TOptions> CreateOptionsCurrent<TOptions>(IOptionsMonitor<TOptions> optionsMonitor) 
        where TOptions : class 
        => new OptionsCurrent<TOptions>(optionsMonitor);

    /// <summary>
    /// Create an <see cref="IOptionsValue{TOptions}"/> for unit-testing.
    /// </summary>
    /// <typeparam name="TOptions">The options type (class)</typeparam>
    /// <param name="option">The option instance.</param>
    /// <returns>an new instance of <see cref="IOptionsValue{TOptions}{TOptions}"/>.</returns>
    public static IOptionsValue<TOptions> CreateOptionsValue<TOptions>(TOptions option) 
        where TOptions : class
        => CreateOptionsCurrent(option).GetOptionsValue();

    /// <summary>
    /// Create an <see cref="IOptionsValue{TOptions}"/> for unit-testing.
    /// </summary>
    /// <typeparam name="TOptions">The options type (class)</typeparam>
    /// <param name="optionsMonitor">The source of the option</param>
    /// <returns>an new instance of <see cref="IOptionsValue{TOptions}{TOptions}"/>.</returns>
    public static IOptionsValue<TOptions> CreateOptionsValue<TOptions>(IOptionsMonitor<TOptions> optionsMonitor) 
        where TOptions : class
        => CreateOptionsCurrent(optionsMonitor).GetOptionsValue();
}
