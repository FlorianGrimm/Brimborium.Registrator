namespace Brimborium.Registrator;

internal sealed class OptionsValue<TOptions> : IOptionsValue<TOptions> where TOptions : class {
    private readonly IOptionsCurrent<TOptions> _OptionsCurrent;
    private TOptions _Value;
    private long _Version;

    public OptionsValue(IOptionsCurrent<TOptions> optionsCurrent) {
        this._OptionsCurrent = optionsCurrent;
        (this._Version, this._Value) = optionsCurrent.GetCurrent();
    }

    public TOptions CurrentValue {
        get {
            if (this._Version == this._OptionsCurrent.Version) {
                return this._Value;
            } else {
                (this._Version, this._Value) = this._OptionsCurrent.GetCurrent();
                return this._Value;
            }
        }
    }

    public TOptions Value => this._Value;

    public long Version => this._Version;
}
