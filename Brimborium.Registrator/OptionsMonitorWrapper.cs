namespace Brimborium.Registrator;

/// <summary>
/// An IOptionsMonitor for Unit testing no update no named options.
/// </summary>
/// <typeparam name="TOptions"></typeparam>
internal sealed class OptionsMonitorWrapper<TOptions> : IOptionsMonitor<TOptions>
    where TOptions : class {
    private TOptions _Option;

    public OptionsMonitorWrapper(TOptions option) {
        _Option = option;
    }

    public TOptions CurrentValue => _Option;

    public TOptions Get(string? name) {
        return _Option;
    }

    public IDisposable? OnChange(Action<TOptions, string> listener) {
        return null;
    }
}