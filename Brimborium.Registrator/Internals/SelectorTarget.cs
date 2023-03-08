namespace Brimborium.Registrator;

internal sealed class SelectorTarget : ISelectorTarget {
    public List<ServicePopulation> ListServicePopulation { get; }
    public List<OptionPopulation> ListOptionPopulation { get; }

    public SelectorTarget() {
        this.ListServicePopulation = new List<ServicePopulation>();
        this.ListOptionPopulation = new List<OptionPopulation>();
    }

    public void AddServicePopulation(ServicePopulation value) {
        this.ListServicePopulation.Add(value);
    }

    public void AddOptionPopulation(OptionPopulation value) {
        this.ListOptionPopulation.Add(value);
    }

    public void Build(IServiceCollection services) {
        var groupsServicePopulation = this.ListServicePopulation.GroupBy(sp => sp.ImplementationType);
        foreach (var grpServicePopulation in groupsServicePopulation) {
            var implementationType = grpServicePopulation.Key;
            if (grpServicePopulation.Count() == 1) {
                var servicePopulation = grpServicePopulation.First();
                var strategy = (servicePopulation.Strategy ?? RegistrationStrategy.Append);
                if (!servicePopulation.IsIncludeSelf
                    && (servicePopulation.ServiceTypes.Length == 1)) {
                    var descriptor = new ServiceDescriptor(
                        servicePopulation.ServiceTypes[0],
                        implementationType,
                        servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                    strategy.Apply(services, descriptor);
                } else {
                    {
                        var descriptor = new ServiceDescriptor(
                            implementationType,
                            implementationType,
                            servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                        strategy.Apply(services, descriptor);
                    }
                    if (servicePopulation.ServiceTypes.Length > 0) {
                        Func<IServiceProvider, object>? factory = null;
                        foreach (var serviceType in servicePopulation.ServiceTypes) {
                            if (serviceType.Equals(implementationType)) {
                                /*
                                var descriptor = new ServiceDescriptor(
                                    implementationType,
                                    implementationType,
                                    servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                                strategy.Apply(services, descriptor);
                                */
                            } else {
                                if (factory is null) {
                                    factory = (new FactoryOfType(implementationType)).factory;
                                }
                                var descriptor = new ServiceDescriptor(
                                    serviceType,
                                    factory,
                                    servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                                strategy.Apply(services, descriptor);
                            }
                        }
                    }
                }
            } else {
                var key = grpServicePopulation.Key;
                var hsServiceTypes = new HashSet<Type>();
                var isIncludeSelf = false;
                foreach (var servicePopulation in grpServicePopulation) {
                    if (servicePopulation.IsIncludeSelf) {
                        isIncludeSelf = true;
                    }
                    foreach (var serviceType in servicePopulation.ServiceTypes) {
                        hsServiceTypes.Add(serviceType);
                    }
                }
                foreach (var servicePopulation in grpServicePopulation) {
                    var strategy = (servicePopulation.Strategy ?? RegistrationStrategy.Append);
                    if (!isIncludeSelf
                        && (hsServiceTypes.Count == 1)) {
                        var descriptor = new ServiceDescriptor(
                            servicePopulation.ServiceTypes[0],
                            implementationType,
                            servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                        strategy.Apply(services, descriptor);
                    } else {
                        {
                            var descriptor = new ServiceDescriptor(
                                implementationType,
                                implementationType,
                                servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                            strategy.Apply(services, descriptor);
                        }
                        if (hsServiceTypes.Count > 0) {
                            Func<IServiceProvider, object>? factory = null;
                            foreach (var serviceType in hsServiceTypes) {
                                if (serviceType.Equals(implementationType)) {
                                    /*
                                    var descriptor = new ServiceDescriptor(
                                        implementationType,
                                        implementationType,
                                        servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                                    strategy.Apply(services, descriptor);
                                    */
                                } else {
                                    if (factory is null) {
                                        factory = (new FactoryOfType(implementationType)).factory;
                                    }
                                    var descriptor = new ServiceDescriptor(
                                        serviceType,
                                        factory,
                                        servicePopulation.Lifetime.GetValueOrDefault(ServiceLifetime.Transient));
                                    strategy.Apply(services, descriptor);
                                }
                            }
                        }
                    }
                }
            }
        }

        //
        if (this.ListOptionPopulation.Any()) {
            services.AddOptions();
            foreach (var optionPopulation in this.ListOptionPopulation) {
                if (optionPopulation.UsingOptionsCurrent) {
                    var miConfigureOption = optionPopulation.OptionType.GetMethod("Configure", BindingFlags.Static | BindingFlags.Public);
                    if (miConfigureOption is null) {
                        typeof(FactoryBuildOptionsCurrent<>).MakeGenericType(optionPopulation.OptionType).InvokeMember(
                            "AddAppOptions",
                            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                            null,
                            null,
                            new object[] { services, optionPopulation.Section }
                        );
                    } else {
                        miConfigureOption.Invoke(null, new object[] { services });
                    }
                } else {
                    var miConfigureOption = optionPopulation.OptionType.GetMethod("Configure", BindingFlags.Static | BindingFlags.Public);
                    if (miConfigureOption is null) {
                        typeof(FactoryBuildOptions<>).MakeGenericType(optionPopulation.OptionType).InvokeMember(
                            "AddAppOptions",
                            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                            null,
                            null,
                            new object[] { services, optionPopulation.Section }
                        );
                    } else {
                        miConfigureOption.Invoke(null, new object[] { services });
                    }
                }
            }
        }
    }

    private record FactoryOfType(Type Type) {
        public object factory(IServiceProvider provider) {
            return provider.GetService(Type)!;
        }
    }
    
    private static class FactoryBuildOptions<TOption> where TOption : class {
        public static void AddAppOptions(IServiceCollection services, string section) {
            services.AddOptions<TOption>().BindConfiguration(section);
        }
    }

    private static class FactoryBuildOptionsCurrent<TOption> where TOption : class {
        public static void AddAppOptions(IServiceCollection services, string section) {
            services.AddOptions<TOption>().BindConfiguration(section).AddOptionsCurrent();
        }
    }
}
