namespace Brimborium.Registrator.SampleSimple;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Text;
using global::System.Threading.Tasks;
using global::Brimborium.Registrator;
using global::Microsoft.Extensions.DependencyInjection;

public interface IMyService {
    void Greeting();
}

[Brimborium.Registrator.Singleton()]
// [Brimborium.Registrator.Singleton(typeof(IMyService))]
// [Brimborium.Registrator.Transient()]
// [Brimborium.Registrator.Transient(typeof(IMyService))]
// [Brimborium.Registrator.ServiceDescriptor(typeof(IMyService), ServiceLifetime.Singleton, ServiceTypeMode.Auto)]
// [Brimborium.Registrator.ServiceDescriptor(typeof(IMyService), ServiceLifetime.Singleton, ServiceTypeMode.IncludeInterfaces)]

public class MyService:IMyService {
    public void Greeting() {
        Console.WriteLine("Hello, World!");
    }
}

public static class Program {
    public static void Main(string[] args) {
        var services = new ServiceCollection();
        services.AddServicesWithRegistrator();

        using var provider = services.BuildServiceProvider();
        var myService = provider.GetService<MyService>();
        if (myService is null){
            System.Console.Out.WriteLine("myService is null");
        } else {
            System.Console.Out.WriteLine("myService.Greeting()");
            myService.Greeting();
        }

        var imyService = provider.GetService<IMyService>();
        if (imyService is null){
            System.Console.Out.WriteLine("imyService is null");
        } else {
            System.Console.Out.WriteLine("imyService.Greeting()");
            imyService.Greeting();
        }

        System.Console.Out.WriteLine($"ReferenceEquals: {ReferenceEquals(myService, imyService)}");
    }
}

