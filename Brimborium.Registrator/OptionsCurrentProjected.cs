namespace Brimborium.Registrator;

internal sealed class OptionsCurrentProjected<TOptionsSource, TOptions>
    : IOptionsCurrent<TOptions>
    , IDisposable

    where TOptionsSource : class
    where TOptions : class {
    private readonly IOptionsCurrent<TOptionsSource> _OptionsSource;
    private readonly Func<TOptionsSource, TOptions> _Project;
    private long _Version;
    private TOptionsSource _OptionsSourceValue;
    private TOptions _OptionsValue;

    internal OptionsCurrentProjected(
        IOptionsCurrent<TOptionsSource> optionsSource,
        Func<TOptionsSource, TOptions> project
        ) {
        this._OptionsSource = optionsSource;
        this._Project = project;
        (this._Version, this._OptionsSourceValue) = this._OptionsSource.GetCurrent();
        this._OptionsValue = project(this._OptionsSourceValue);
    }

    public TOptions CurrentValue => this.GetCurrent().currentValue;

    public long Version => this._Version;

    public (long version, TOptions currentValue) GetCurrent() {
        if (this._Version == this._OptionsSource.Version) {
            return (version: this._Version, currentValue: this._OptionsValue);
        } else {
            (this._Version, this._OptionsSourceValue) = this._OptionsSource.GetCurrent();
            return (version: this._Version, currentValue: this._OptionsValue);
        }
    }

    public IOptionsValue<TOptions> GetOptionsValue()
        => new OptionsValue<TOptions>(this);

    public void Dispose() {
        this._OptionsSource.Dispose();
    }

    public IOptionsCurrent<TOptionsTarget> GetOptionsCurrentProjected<TOptionsTarget>(Func<TOptions, TOptionsTarget> projectValue)
        where TOptionsTarget : class
        => new OptionsCurrentProjected<TOptions, TOptionsTarget>(this, projectValue);
}
