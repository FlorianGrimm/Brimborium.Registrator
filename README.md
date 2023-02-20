# Brimborium.Registrator

dotnet core 7

work in progress

based on these
https://github.com/khellang/Scrutor
https://github.com/salman-basmechi/reguto

## Sample

```C#
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
        services.AddServicesWithRegistrator();

        using var provider = services.BuildServiceProvider();
        
        var myService = provider.GetRequiredService<MyService>();
        myService.Greeting();

        var imyService = provider.GetRequiredService<IMyService>();
        imyService.Greeting();

        System.Console.Out.WriteLine(ReferenceEquals(myService, imyService));
    }
}
```