namespace Brimborium.Registrator;

internal class OptionTypeSelector : IOptionTypeSelector, IImplementationTypeSelector, ISelector {
    private readonly IImplementationTypeSelector _Inner;
    private readonly IEnumerable<Type> _Types;
    private List<ISelector>? _ListSelector;
    private List<OptionPopulation>? _ListOptionPopulation;

    public OptionTypeSelector(IImplementationTypeSelector inner, IEnumerable<Type> types) {
        this._Inner = inner;
        this._Types = types;
    }

    public IOptionTypeSelector AddOptions() {
        var listOptionPopulation = this._ListOptionPopulation ??= new List<OptionPopulation>();
        foreach (var type in this._Types) {
            var attr = type.GetCustomAttribute<OptionsAttribute>();
            if (attr is null) {
                continue;
            }
            listOptionPopulation.Add(new OptionPopulation(type, false, attr.Section));
        }
        return this;
    }

    public IOptionTypeSelector AddOptions(Action<IImplementationTypeFilter> action, bool publicOnly) {
        var filter = new ImplementationTypeFilter(this._Types);
        action(filter);
        var selector = new OptionCurrentTypeSelector(this._Inner, filter.Types);
        (this._ListSelector ??= new List<ISelector>()).Add(selector);
        return selector;
    }

    public IOptionTypeSelector AddOptionsCurrent() {
        return this._Inner.AddOptionsCurrent();
    }

    public IOptionTypeSelector AddOptionsCurrent(Action<IImplementationTypeFilter> action, bool publicOnly) {
        return this._Inner.AddOptionsCurrent(action, publicOnly);
    }

    #region Chain Methods

    public IServiceTypeSelector AddClasses() {
        return this._Inner.AddClasses();
    }

    public IServiceTypeSelector AddClasses(bool publicOnly) {
        return this._Inner.AddClasses(publicOnly);
    }

    public IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action) {
        return this._Inner.AddClasses(action);
    }

    public IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action, bool publicOnly) {
        return this._Inner.AddClasses(action, publicOnly);
    }

    public IImplementationTypeSelector FromApplicationDependencies(DependencyContext context, Func<AssemblyName, bool>? predicate = null) {
        return this._Inner.FromApplicationDependencies(context, predicate);
    }

    public IImplementationTypeSelector FromAssemblies(params Assembly[] assemblies) {
        return this._Inner.FromAssemblies(assemblies);
    }

    public IImplementationTypeSelector FromAssemblies(IEnumerable<Assembly> assemblies) {
        return this._Inner.FromAssemblies(assemblies);
    }

    public IImplementationTypeSelector FromAssembliesOf(params Type[] types) {
        return this._Inner.FromAssembliesOf(types);
    }

    public IImplementationTypeSelector FromAssembliesOf(IEnumerable<Type> types) {
        return this._Inner.FromAssembliesOf(types);
    }

    public IImplementationTypeSelector FromAssemblyDependencies(DependencyContext context, Assembly assembly, Func<AssemblyName, bool>? predicate = null) {
        return this._Inner.FromAssemblyDependencies(context, assembly, predicate);
    }

    public IImplementationTypeSelector FromAssemblyOf<T>() {
        return this._Inner.FromAssemblyOf<T>();
    }

    public IImplementationTypeSelector FromDependencyContext(DependencyContext context, Func<AssemblyName, bool>? predicate = null) {
        return this._Inner.FromDependencyContext(context, predicate);
    }
    #endregion

    void ISelector.Populate(RegistrationStrategy registrationStrategy, ISelectorTarget selectorTarget) {
        if (this._ListOptionPopulation is null) {
            this.AddOptions();
        }
        if (this._ListOptionPopulation is not null) {
            selectorTarget.ListOptionPopulation.AddRange(this._ListOptionPopulation);
        }

        if (this._ListSelector is not null) {
            foreach (var selector in this._ListSelector) {
                selector.Populate(registrationStrategy, selectorTarget);
            }
        }
    }
}
