namespace Brimborium.Registrator;

public static class OptionsCurrentExtensions {
    public static IServiceCollection AddOptionsCurrent<TOptions>(this IServiceCollection services)
        where TOptions : class {
        services.AddOptions<TOptions>().AddOptionsCurrent<TOptions>();
        return services;
    }

    public static IServiceCollection AddOptionsCurrentProject<TOptionsSource, TOptions>(this IServiceCollection services, Func<TOptionsSource, TOptions> projectValue)
        where TOptionsSource : class
        where TOptions : class {
        services.AddOptions<TOptions>();
        services.AddSingleton<IOptionsCurrent<TOptions>>((sp) => {
            var options = sp.GetRequiredService<IOptionsCurrent<TOptionsSource>>();
            return options.GetOptionsCurrentProjected<TOptions>(projectValue);
        });
        services.AddTransient<IOptionsValue<TOptions>>((sp) => sp.GetRequiredService<OptionsCurrent<TOptions>>().GetOptionsValue());
        return services;
    }

    public static OptionsBuilder<TOptions> AddOptionsCurrent<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class {
        //optionsBuilder.Services.AddOptionsCurrent<TOptions>();
        optionsBuilder.Services.AddSingleton<IOptionsCurrent<TOptions>>((sp) => sp.GetRequiredService<OptionsCurrent<TOptions>>());
        optionsBuilder.Services.AddSingleton<OptionsCurrent<TOptions>>((sp) => {
            var options = sp.GetRequiredService<IOptionsMonitor<TOptions>>();
            return new OptionsCurrent<TOptions>(options);
        });
        optionsBuilder.Services.AddTransient<IOptionsValue<TOptions>>((sp) => sp.GetRequiredService<OptionsCurrent<TOptions>>().GetOptionsValue());

        return optionsBuilder;
    }
}
