# Brimborium.Registrator

Version 1.0.0.0

dotnet core 7


## Sample Dependency Injection with Registrator - Attribute based

```csharp
using Brimborium.Registrator;
using Microsoft.Extensions.DependencyInjection;

public interface IMyService {
    void Greeting();
}

[Brimborium.Registrator.Singleton()]
public class MyService:IMyService{
    public void Greeting() {
        Console.WriteLine("Hello, World!");
    }
}
public static class Program {
    public static void Main(string[] args) {
        var services = new ServiceCollection();
        services.AddServicesWithRegistrator(); /* <-- scan the assemblies for classes with an attribute*/

        using var provider = services.BuildServiceProvider();
        
        var myService = provider.GetRequiredService<MyService>();
        myService.Greeting();

        var imyService = provider.GetRequiredService<IMyService>();
        imyService.Greeting();

        System.Console.Out.WriteLine(ReferenceEquals(myService, imyService));
    }
}
```

The attributes are:
- Singleton
- Scoped
- Transient
- ServiceDescriptor

In the default mode the class and the interfaces a registered.

In the example abouve these 2 services are registered.
  1) Singleton Brimborium.Registrator.SampleSimple.MyService ImplementationType: Brimborium.Registrator.SampleSimple.MyService
  2) Singleton Brimborium.Registrator.SampleSimple.IMyService ImplementationFactory

This factory in 2) uses MyService - provider.GetService<MyService>() to get the instance.
So both 1) and 2) returns the same instance.

If you want to register only one interface you can write
```csharp
[Brimborium.Registrator.Singleton(typeof(IMyService))]
public class MyService : IMyService, IMyOtherService {
}
```csharp

If you want to register all interfaces you can write
```csharp

[Brimborium.Registrator.Singleton(Mode = ServiceTypeMode.IncludeInterfaces)]
public class MyService : IMyService, IMyOtherService {
}
```csharp

## Sample Options with Registrator - Attribute based

```csharp
using Brimborium.Registrator;
using Microsoft.Extensions.DependencyInjection;

[Brimborium.Registrator.Options()]
public class MyOneOptions {
    public string? Gna { get; set; }
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
        
        services.AddServicesWithRegistrator();
        
        using var provider = services.BuildServiceProvider();

        var myOneOptions = provider.GetService<IOptions<MyOneOptions>>()?.Value;
        if (myOneOptions is null) {
            System.Console.Out.WriteLine("myOneOptions is null");
        } else {
            System.Console.Out.WriteLine($"myOneOptions.Gna: {myOneOptions.Gna}");
        }
    }
}
```


## Sample OptionsCurrent with Registrator - Attribute based

```csharp
using Brimborium.Registrator;
using Microsoft.Extensions.DependencyInjection;

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
        
        services.AddServicesWithRegistrator();
        
        using var provider = services.BuildServiceProvider();

        var myOtherOptions = provider.GetService<IOptionsCurrent<MyOtherOptions>>()?.CurrentValue;
        if (myOtherOptions is null) {
            System.Console.Out.WriteLine("myOtherOptions is null");
        } else {
            System.Console.Out.WriteLine($"myOtherOptions.Ebbes: {myOtherOptions.Ebbes}");
        }
    }
}
```

## AddServicesWithRegistrator

Scan all assemblies FromDependencyContext.

```csharp
        services.AddServicesWithRegistrator();
```

ignore the assembly if assemblyName.Name
  - .Equals("System"
  - .Equals("mscorlib")
  - .Equals("netstandard")
  - .Equals("WindowsBase")
  - .StartsWith("System.")
  - .StartsWith("Microsoft.CodeAnalysis")
  - .StartsWith("Microsoft.")
  - .Equals("Brimborium.Registrator")
  - .Equals("Brimborium.Registrator.Abstractions")

or customize
```csharp
        services.AddServicesWithRegistrator(
            (typeSourceSelector) => {
                var dependencyContext = Microsoft.Extensions.DependencyModel.DependencyContext.Default;
                if (dependencyContext is null) { return }
                var assemblySourceSelector = typeSourceSelector.FromApplicationDependencies(
                        dependencyContext,
                        (assemblyName) => (assemblyName.Name is string name)
                                // replace this with your magic to limit the types that are queried - it's not cheap to load an assembly for not using it
                                && (name.StartsWith("Brimborium.", StringComparison.Ordinal))
                );
                assemblySourceSelector.AddClasses(a => a.WithAttribute<ServiceDescriptorAttribute>()).UsingAttributes();
            });
```

# OptionsCurrent

I had the problem with IOptions<TOptions> and dynamic updates. 
You must use an IOptionMonitor and you must use the OnChange to trigger the changes.

The IOptionsCurrent<TOption> is a wrapper around IOptionsMonitor<TOptions> to get the current value.
It also uses OnChange to the the changes to the current value.

The IOptionsValue uses IOptionsCurrent to get the current option value.

An potential issue if you have an algorythm that runs for a few seconds and you change the options in the meantime.
Theirfore you can use the IOptionsValue<TOptions> to control when the update becomes visible
  - get the latest CurrentValue 
  - then get the same (may be old) Value .

```csharp
namespace Brimborium.Registrator;

/// <summary>
/// Using an IOptionsMonitor to provide a snapshot of the current value of the options.
/// This is normally a singelton that also uses OnChange to update the current value.
/// </summary>
/// <typeparam name="TOptions">The options type</typeparam>
public interface IOptionsCurrent<TOptions>
    : IDisposable
    where TOptions : class {
    /// <summary>
    /// The current latest value.
    /// </summary>
    TOptions CurrentValue { get; }

    // ..snip..
}

/// <summary>
/// Using a IOptionsCurrent to provide a snapshot of the current value of the options.
/// You can control if you want the current value or the not updated value.
/// </summary>
/// <typeparam name="TOptions">The options type</typeparam>
public interface IOptionsValue<TOptions> where TOptions : class {
    /// <summary>
    /// The current latest value.
    /// </summary>
    TOptions CurrentValue { get; }

    /// <summary>
    /// The value of initalization or of the last time 'you' called CurrentValue.
    /// </summary>
    TOptions Value { get; }
    // ..snip..
}

```




## based on these libraries

  - https://github.com/khellang/Scrutor
  - https://github.com/salman-basmechi/reguto



Happy coding