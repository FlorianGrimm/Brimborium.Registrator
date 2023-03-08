

namespace Brimborium.Registrator.Test {
    public class BasicTest {
        [Fact]
        public void AddClassesSelf() {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddServicesWithRegistrator(
                a => {
                    a.FromAssemblyDependencies(
                        DependencyContext.Default ?? throw new Exception("DependencyContext.Default is null")
                        , typeof(BasicTest).Assembly, (assName) => (assName.Name ?? "").StartsWith("Brimborium"))
                        .AddClasses()
                        .AsSelf()
                    ;
                }, st => {
                    var implementationTypes = st.ListServicePopulation.Select(i => i.ImplementationType).ToArray();
                    Assert.Contains(implementationTypes, i => typeof(DummyImpl1).Equals(i));
                });
        }

        [Fact]
        public void AddClassesSelfWithoutAttribute() {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddServicesWithRegistrator(
                a => {
                    a.FromAssemblyDependencies(
                        DependencyContext.Default ?? throw new Exception("DependencyContext.Default is null")
                        , typeof(BasicTest).Assembly, (assName) => (assName.Name ?? "").StartsWith("Brimborium"))
                        .AddClasses(classes => classes.WithoutAttribute<ServiceDescriptorAttribute>())
                        .AsSelf()
                    ;
                }, st => {
                    var implementationTypes = st.ListServicePopulation.Select(i => i.ImplementationType).ToArray();
                    Assert.Contains(implementationTypes, i => typeof(DummyImpl1).Equals(i));
                    Assert.DoesNotContain(implementationTypes, i => typeof(DummyImpl1AttributeSingleton).Equals(i));
                });
        }


        [Fact]
        public void AddClassesAsImplementedInterfaces() {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddServicesWithRegistrator(
                a => {
                    a.FromAssemblyDependencies(
                        DependencyContext.Default ?? throw new Exception("DependencyContext.Default is null"),
                        typeof(BasicTest).Assembly, (assName) => (assName.Name ?? "").StartsWith("Brimborium"))
                        .AddClasses(classes => classes.WithAttribute<ServiceDescriptorAttribute>())
                        .AsImplementedInterfaces();
                    ;
                }, st => {
                    var implementationTypes = st.ListServicePopulation.Select(i => i.ImplementationType).ToArray();
                    Assert.Contains(implementationTypes, i => typeof(DummyImpl1AttributeSingleton).Equals(i));
                    Assert.DoesNotContain(implementationTypes, i => typeof(BasicTest).Equals(i));
                });
        }

        [Fact]
        public void AddClassesNever() {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddServicesWithRegistrator(
                a => {
                    a.FromAssemblyDependencies(
                        DependencyContext.Default ?? throw new Exception("DependencyContext.Default is null"),
                        typeof(BasicTest).Assembly, (assName) => (assName.Name ?? "").StartsWith("Brimborium"))
                        .AddClasses(classes => classes.WithoutAttribute<ServiceDescriptorAttribute>().WithAttribute<ServiceDescriptorAttribute>())

                    ;
                });
            Assert.Empty(services);
        }


        [Fact]
        public void AddClassesAsMatchingInterfaceFalse() {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddServicesWithRegistrator(
                a => {
                    a.FromAssemblyDependencies(
                        DependencyContext.Default ?? throw new Exception("DependencyContext.Default is null"),
                        typeof(BasicTest).Assembly, (assName) => (assName.Name ?? "").StartsWith("Brimborium"))
                        .AddClasses()
                        .AsMatchingInterface((t, a) => { a.Where(x => false); });
                    ;
                });
            Assert.Empty(services);
        }


        [Fact]
        public void AddClassesAsInterface() {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddServicesWithRegistrator(
                a => {
                    a.FromAssemblyDependencies(
                        DependencyContext.Default ?? throw new Exception("DependencyContext.Default is null"),
                         typeof(BasicTest).Assembly, (assName) => (assName.Name ?? "").StartsWith("Brimborium"))
                        .AddClasses(classes => classes.WithoutAttribute<ServiceDescriptorAttribute>())
                        .As<IA>();
                    ;
                });
            Assert.NotEmpty(services);
            Assert.All(services, sd => { Assert.True(sd.ServiceType == typeof(IA)); });
        }
    }
}
