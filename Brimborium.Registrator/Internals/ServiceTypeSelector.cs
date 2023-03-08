namespace Brimborium.Registrator;

internal class ServiceTypeSelector : IServiceTypeSelector, ISelector {
    private readonly IImplementationTypeSelector _Inner;
    private readonly IEnumerable<Type> _Types;
    private List<ISelector>? _ListSelector;
    private RegistrationStrategy? _RegistrationStrategy;

    public ServiceTypeSelector(IImplementationTypeSelector inner, IEnumerable<Type> types) {
        this._Inner = inner;
        this._Types = types;
    }

    public ILifetimeSelector AsSelf() {
        return this.As(t => new[] { t });
    }

    public ILifetimeSelector As<T>() {
        return this.As(typeof(T));
    }

    public ILifetimeSelector As(params Type[] types) {
        Preconditions.NotNull(types, nameof(types));

        return this.AddSelector(
            this._Types
                .Select(t => new { implementationType = t, serviceTypes = types.Where(i => t.IsAssignableTo(i)).ToArray() })
                .Where(t => t.serviceTypes.Length > 0)
                .Select(t => ServicePopulation.Create(t.implementationType, t.serviceTypes, false, null, null))
            );
    }

    public ILifetimeSelector As(IEnumerable<Type> types) {
        Preconditions.NotNull(types, nameof(types));

        return this.AddSelector(
            this._Types
                .Select(t => new { implementationType = t, serviceTypes = types.Where(i => t.IsAssignableTo(i)).ToArray() })
                .Where(t => t.serviceTypes.Length > 0)
                .Select(t => ServicePopulation.Create(t.implementationType, t.serviceTypes, false, null, null))
            );
    }

    public ILifetimeSelector AsImplementedInterfaces() {
        return this.AsTypeInfo(t => t.ImplementedInterfaces
            .Where(x => x.HasMatchingGenericArity(t))
            .Select(x => x.GetRegistrationType(t)));
    }

    public ILifetimeSelector AsSelfWithInterfaces() {
        IEnumerable<Type> Selector(TypeInfo info) {
            if (info.IsGenericTypeDefinition) {
                // This prevents trying to register open generic types
                // with an ImplementationFactory, which is unsupported.
                return Enumerable.Empty<Type>();
            }

            return info.ImplementedInterfaces
                .Where(x => x.HasMatchingGenericArity(info))
                .Select(x => x.GetRegistrationType(info));
        }

        return this.AddSelector(
            this._Types
                .Select(t => new { implementationType = t, serviceTypes = Selector(t.GetTypeInfo()).ToArray() })
                .Where(t => t.serviceTypes.Any())
                .Select(t => new ServicePopulation(t.implementationType, t.serviceTypes, true, null, null)));
    }

    public ILifetimeSelector AsMatchingInterface() {
        return this.AsMatchingInterface(null);
    }

    public ILifetimeSelector AsMatchingInterface(Action<TypeInfo, IImplementationTypeFilter>? action) {
        return this.AsTypeInfo(t => t.FindMatchingInterface(action));
    }

    public ILifetimeSelector As(Func<Type, IEnumerable<Type>> selector) {
        Preconditions.NotNull(selector, nameof(selector));

        return this.AddSelector(
            this._Types
                .Select(t => new { implementationType = t, serviceTypes = selector(t).ToArray() })
                .Where(t => t.serviceTypes.Any())
                .Select(t => new ServicePopulation(t.implementationType, t.serviceTypes, false, null, null))
            );
    }

    public IImplementationTypeSelector UsingAttributes(Func<Type, Type, bool>? predicate = default) {
        var selector = new AttributeSelector(this._Types, predicate);

        (this._ListSelector ??= new List<ISelector>()).Add(selector);

        return this;
    }

    public IServiceTypeSelector UsingRegistrationStrategy(RegistrationStrategy registrationStrategy) {
        Preconditions.NotNull(registrationStrategy, nameof(registrationStrategy));

        this._RegistrationStrategy = registrationStrategy;
        return this;
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

    public IOptionTypeSelector AddOptions() {
        return this._Inner.AddOptions();
    }

    public IOptionTypeSelector AddOptions(Action<IImplementationTypeFilter> action, bool publicOnly) {
        return this._Inner.AddOptions(action, publicOnly);
    }

    public IOptionTypeSelector AddOptionsCurrent() {
        return this._Inner.AddOptionsCurrent();
    }

    public IOptionTypeSelector AddOptionsCurrent(Action<IImplementationTypeFilter> action, bool publicOnly) {
        return this._Inner.AddOptionsCurrent(action, publicOnly);
    }

    #endregion

    internal void PropagateLifetime(ServiceLifetime lifetime) {
        if (this._ListSelector is not null) {
            foreach (var selector in this._ListSelector.OfType<LifetimeSelector>()) {
                selector.Lifetime = lifetime;
            }
        }
    }

    void ISelector.Populate(RegistrationStrategy registrationStrategy, ISelectorTarget selectorTarget) {
        if (this._ListSelector is null) {
            this.AsSelf();
        }

        if (this._ListSelector is not null) {
            var strategy = this._RegistrationStrategy ?? registrationStrategy;
            foreach (var selector in this._ListSelector) {
                selector.Populate(strategy, selectorTarget);
            }
        }
    }

    private ILifetimeSelector AddSelector(IEnumerable<ServicePopulation> items) {
        var selector = new LifetimeSelector(this, items switch { 
            List<ServicePopulation> => items,
            _ when items.Any() => new List<ServicePopulation>(items),
            _=> Array.Empty<ServicePopulation>()
        });

        (this._ListSelector ??= new List<ISelector>()).Add(selector);

        return selector;
    }

    private ILifetimeSelector AsTypeInfo(Func<TypeInfo, IEnumerable<Type>> selector) {
        return this.As(t => selector(t.GetTypeInfo()));
    }
}
