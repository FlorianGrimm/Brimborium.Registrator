namespace Brimborium.Registrator.Internals;

internal class ImplementationTypeSelector : IImplementationTypeSelector, ISelector {
    private readonly ITypeSourceSelector _Inner;
    private readonly IEnumerable<Type> _Types;
    private List<ISelector>? _ListSelector;

    public ImplementationTypeSelector(ITypeSourceSelector inner, IEnumerable<Type> types) {
        this._Inner = inner;
        this._Types = types;
    }

    public IServiceTypeSelector AddClasses() {
        return this.AddClasses(publicOnly: true);
    }

    public IServiceTypeSelector AddClasses(bool publicOnly) {
        var classes = this.GetNonAbstractClasses(publicOnly);

        var selector = new ServiceTypeSelector(this, classes);
        (this._ListSelector ??= new List<ISelector>()).Add(selector);
        return selector;
    }

    public IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action) {
        return this.AddClasses(action, publicOnly: false);
    }

    public IOptionTypeSelector AddOptions() {
        var classes = this.GetNonAbstractClasses(true);
        var filter = new ImplementationTypeFilter(classes);
        filter.WithAttribute<OptionsAttribute>();
        var selector = new OptionTypeSelector(this, filter.Types);
        (this._ListSelector??=new List<ISelector>()).Add(selector);
        return selector;
    }

    public IOptionTypeSelector AddOptions(Action<IImplementationTypeFilter> action, bool publicOnly) {
        var classes = this.GetNonAbstractClasses(publicOnly);
        var filter = new ImplementationTypeFilter(classes);
        if (action is not null) {
            action(filter);
        } else {
            filter.WithAttribute<OptionsAttribute>();
        }
        var selector = new OptionTypeSelector(this, filter.Types);
        (this._ListSelector ??= new List<ISelector>()).Add(selector);
        return selector;
    }

    public IOptionTypeSelector AddOptionsCurrent() {
        var classes = this.GetNonAbstractClasses(true);
        var filter = new ImplementationTypeFilter(classes);
        filter.WithAttribute<OptionsCurrentAttribute>();
        var selector = new OptionCurrentTypeSelector(this, filter.Types);
        (this._ListSelector ??= new List<ISelector>()).Add(selector);
        return selector;
    }

    public IOptionTypeSelector AddOptionsCurrent(Action<IImplementationTypeFilter> action, bool publicOnly) {
        var classes = this.GetNonAbstractClasses(publicOnly);
        var filter = new ImplementationTypeFilter(classes);
        if (action is not null) {
            action(filter);
        } else {
            filter.WithAttribute<OptionsCurrentAttribute>();
        }
        var selector = new OptionCurrentTypeSelector(this, filter.Types);
        (this._ListSelector ??= new List<ISelector>()).Add(selector);
        return selector;
    }

    public IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action, bool publicOnly) {
        Preconditions.NotNull(action, nameof(action));

        var classes = this.GetNonAbstractClasses(publicOnly);
        var filter = new ImplementationTypeFilter(classes);
        action(filter);
        var selector = new ServiceTypeSelector(this, filter.Types);
        (this._ListSelector ??= new List<ISelector>()).Add(selector);
        return selector;
    }

    #region Chain Methods

    public IImplementationTypeSelector FromApplicationDependencies(DependencyContext context, Func<AssemblyName, bool>? predicate = default) {
        return this._Inner.FromApplicationDependencies(context, predicate);
    }

    public IImplementationTypeSelector FromAssemblyDependencies(DependencyContext context, Assembly assembly, Func<AssemblyName, bool>? predicate = default) {
        return this._Inner.FromAssemblyDependencies(context, assembly, predicate);
    }

    public IImplementationTypeSelector FromDependencyContext(DependencyContext context, Func<AssemblyName, bool>? predicate = default) {
        return this._Inner.FromDependencyContext(context, predicate);
    }

    public IImplementationTypeSelector FromAssemblyOf<T>() {
        return this._Inner.FromAssemblyOf<T>();
    }

    public IImplementationTypeSelector FromAssembliesOf(params Type[] types) {
        return this._Inner.FromAssembliesOf(types);
    }

    public IImplementationTypeSelector FromAssembliesOf(IEnumerable<Type> types) {
        return this._Inner.FromAssembliesOf(types);
    }

    public IImplementationTypeSelector FromAssemblies(params Assembly[] assemblies) {
        return this._Inner.FromAssemblies(assemblies);
    }

    public IImplementationTypeSelector FromAssemblies(IEnumerable<Assembly> assemblies) {
        return this._Inner.FromAssemblies(assemblies);
    }

    #endregion

    void ISelector.Populate(RegistrationStrategy registrationStrategy, ISelectorTarget selectorTarget) {
        if (this._ListSelector is null) {
            this.AddClasses();
        }
        if (this._ListSelector is not null) {
            foreach (var selector in this._ListSelector) {
                selector.Populate(registrationStrategy, selectorTarget);
            }
        }
    }
    
    private IEnumerable<Type> GetNonAbstractClasses(bool publicOnly) {
        return this._Types.Where(t => t.IsNonAbstractClass(publicOnly));
    }
}
