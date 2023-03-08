namespace Brimborium.Registrator;

internal sealed class OptionsCurrent<TOptions>
    : IOptionsCurrent<TOptions>
    , IDisposable
    where TOptions : class {
    public static OptionsCurrent<TOptions> Create(TOptions option) {
        return new OptionsCurrent<TOptions>(new OptionsMonitorWrapper<TOptions>(option));
    }

    private readonly IOptionsMonitor<TOptions> _Monitor;
    private long _Version;
    private TOptions _Value;
    private IDisposable? _Unlisten;

    internal OptionsCurrent(IOptionsMonitor<TOptions> optionsMonitor) {
        this._Version = 1;
        this._Monitor = optionsMonitor;
        this._Value = optionsMonitor.CurrentValue;

        this._Unlisten = optionsMonitor.OnChange((nextValue, name) => {
            this._Version++;
            this._Value = nextValue;
        });
    }

    public TOptions CurrentValue => this._Value;

    public long Version => this._Version;

    public IOptionsMonitor<TOptions> Monitor => this._Monitor;

    public (long version, TOptions currentValue) GetCurrent() => (version: this._Version, currentValue: this._Value);

    public IOptionsValue<TOptions> GetOptionsValue() => new OptionsValue<TOptions>(this);

    public IOptionsCurrent<TOptionsTarget> GetOptionsCurrentProjected<TOptionsTarget>(Func<TOptions, TOptionsTarget> projectValue)
        where TOptionsTarget : class
        => new OptionsCurrentProjected<TOptions, TOptionsTarget>(this, projectValue);
        
    private void Dispose(bool disposing) {
        using (var l = this._Unlisten) {
            if (disposing) {
                this._Unlisten = null;
            }
        }
    }

    ~OptionsCurrent() {
        Dispose(disposing: false);
    }

    public void Dispose() {
        this.Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}
