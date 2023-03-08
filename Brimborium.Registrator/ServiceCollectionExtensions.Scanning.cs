namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions {
    public static IServiceCollection AddServicesWithRegistrator(
        this IServiceCollection services
        ) {
        return services.AddServicesWithRegistrator(
              actionAdd: (typeSourceSelector) => {
                  var dependencyContext = Microsoft.Extensions.DependencyModel.DependencyContext.Default;
                  if (dependencyContext is not null) {
                      var implementationTypeSelector =
                          typeSourceSelector.FromApplicationDependencies(
                              context: dependencyContext);
                      services.AddAttributtedServices(implementationTypeSelector);
                  }
              },
        actionRevisit: null);
    }

    /// <summary>
    /// Adds registrations to the <paramref name="services"/> collection using
    /// conventions specified using the <paramref name="actionAdd"/>.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="actionAdd">The configuration action.</param>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
    /// or <paramref name="actionAdd"/> arguments are <c>null</c>.</exception>
    public static IServiceCollection AddServicesWithRegistrator(
        this IServiceCollection services,
        Action<ITypeSourceSelector>? actionAdd = default,
        Action<ISelectorTarget>? actionRevisit = default) {
        Preconditions.NotNull(services, nameof(services));

        var selector = new TypeSourceSelector();

        if (actionAdd is not null) {
            actionAdd(selector);
        } else {
            var defaultContext = DependencyContext.Default;
            if (defaultContext is null) {
                throw new InvalidOperationException("DependencyContext.Default is null");
            }
            var typeSelector = selector.FromApplicationDependencies(defaultContext);
            typeSelector.AddClasses().UsingAttributes();
            typeSelector.AddOptions();
            typeSelector.AddOptionsCurrent();
        }

        var selectorTarget = new SelectorTarget();

        ((ISelector)selector).Populate(RegistrationStrategy.Append, selectorTarget);

        if (actionRevisit is not null) {
            actionRevisit(selectorTarget);
        }

        selectorTarget.Build(services);

        return services;
    }
}
