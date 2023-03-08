namespace Brimborium.Registrator.SampleSimple;

using global::Brimborium.Registrator;
using global::Microsoft.Extensions.Configuration;
using global::Microsoft.Extensions.DependencyInjection;
using global::System;

using Microsoft.Extensions.Options;

public interface IMyService {
    void Greeting();
}
public interface IMyOtherService {
    void GoodBye();
}

// some exampels for registration

[Brimborium.Registrator.Singleton()]
// [Brimborium.Registrator.Singleton(typeof(IMyService))]
// [Brimborium.Registrator.Singleton(Mode = ServiceTypeMode.IncludeInterfaces)]
// [Brimborium.Registrator.Transient()]
// [Brimborium.Registrator.Transient(typeof(IMyService))]
// [Brimborium.Registrator.ServiceDescriptor(typeof(IMyService), ServiceLifetime.Singleton, ServiceTypeMode.Auto)]
// [Brimborium.Registrator.ServiceDescriptor(typeof(IMyService), ServiceLifetime.Singleton, ServiceTypeMode.IncludeInterfaces)]

public class MyService : IMyService, IMyOtherService {
    public void Greeting() {
        Console.WriteLine("Hello, World!");
    }
    
    public void GoodBye() {
        Console.WriteLine("cu");
    }
}


[Brimborium.Registrator.Options()]
public class MyOneOptions {
    public string? Gna { get; set; }
}

[Brimborium.Registrator.OptionsCurrent()]
public class MyOtherOptions {
    public string? Ebbes { get; set; }
}

public static class Program {
    public static void Main(string[] args) {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        //services.AddServicesWithRegistrator();
        // -or-
        services.AddServicesWithRegistrator(
            (typeSourceSelector) => {
                var dependencyContext = Microsoft.Extensions.DependencyModel.DependencyContext.Default;
                if (dependencyContext is null) { return; }
                var assemblySourceSelector = typeSourceSelector.FromApplicationDependencies(
                        dependencyContext,
                        (assemblyName) => (assemblyName.Name is string name)
                                // replace this with your magic to limit the types that are queried - it's not cheap to load an assembly for not using it
                                && (name.StartsWith("Brimborium.", StringComparison.Ordinal))
                );
                assemblySourceSelector.AddClasses(a => a.WithAttribute<ServiceDescriptorAttribute>()).UsingAttributes();
                assemblySourceSelector.AddOptions();
                assemblySourceSelector.AddOptionsCurrent();
            });

#if true
        foreach (var sd in services) {
            if (sd.ImplementationType is not null) {
                System.Console.WriteLine($" {sd.Lifetime} {sd.ServiceType.FullName} ImplementationType: {sd.ImplementationType?.FullName}");
                continue;
            }
            if (sd.ImplementationFactory is not null) {
                System.Console.WriteLine($" {sd.Lifetime} {sd.ServiceType.FullName} ImplementationFactory");
                continue;
            }
            if (sd.ImplementationInstance is not null) {
                System.Console.WriteLine($" {sd.Lifetime} {sd.ServiceType.FullName} ImplementationInstance: {sd.ImplementationInstance.GetType().FullName}");
                continue;
            }
        }
#endif

        using var provider = services.BuildServiceProvider();
        var myService = provider.GetService<MyService>();
        if (myService is null) {
            System.Console.Out.WriteLine("myService is null");
        } else {
            System.Console.Out.WriteLine("myService.Greeting()");
            myService.Greeting();
        }

        var imyService = provider.GetService<IMyService>();
        if (imyService is null) {
            System.Console.Out.WriteLine("imyService is null");
        } else {
            System.Console.Out.WriteLine("imyService.Greeting()");
            imyService.Greeting();
        }

        var imyotherService = provider.GetService<IMyOtherService>();
        if (imyotherService is null) {
            System.Console.Out.WriteLine("imyotherService is null");
        } else {
            System.Console.Out.WriteLine("imyotherService.GoodBye()");
            imyotherService.GoodBye();
        }
        System.Console.Out.WriteLine($"ReferenceEquals: myService =?= imyService : {ReferenceEquals(myService, imyService)}");
        System.Console.Out.WriteLine($"ReferenceEquals: myService =?= imyotherService : {ReferenceEquals(myService, imyotherService)}");

        // 
        var myOneOptions = provider.GetService<IOptions<MyOneOptions>>()?.Value;
        if (myOneOptions is null) {
            System.Console.Out.WriteLine("myOneOptions is null");
        } else {
            System.Console.Out.WriteLine($"myOneOptions.Gna: {myOneOptions.Gna}");
        }

        //
        var myOtherOptions = provider.GetService<IOptionsCurrent<MyOtherOptions>>()?.CurrentValue;
        if (myOtherOptions is null) {
            System.Console.Out.WriteLine("myOtherOptions is null");
        } else {
            System.Console.Out.WriteLine($"myOtherOptions.Ebbes: {myOtherOptions.Ebbes}");
        }
    }
}

