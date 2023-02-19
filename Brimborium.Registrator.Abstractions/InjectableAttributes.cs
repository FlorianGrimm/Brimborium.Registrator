﻿using Microsoft.Extensions.DependencyInjection;

using System;

namespace Brimborium.Registrator {

    [Flags]
    public enum ServiceTypeMode {
        /// <summary>
        /// 1) if ServiceType is not null; Add ServiceType 
        /// 2) if ServiceType is null;     Act as IncludeInterfaces
        /// </summary>
        Auto = 0,
        
        /// <summary>
        /// 1) Add ImplementationType as ServiceType 
        /// 2) Add ServiceType if serviceType is not null
        /// </summary>
        IncludeSelf = 1,

        /// <summary>
        /// 1) Add ServiceType if serviceType is not null
        /// 2) Scan for interfaces 
        /// 3) if one interface is found and IncludeSelf is not set      add that as ServiceType
        ///    if more than one interface is found or IncludeSelf is set add ImplementationType as ServiceType; add found types(interfaces) as ServiceType with a factory for ImplementationType
        /// </summary>
        IncludeInterfaces = 2,

        /// <summary>
        /// 1) Add ServiceType if serviceType is not null
        /// 2) Scan for interfaces and base types
        /// 3) if one interface is found and IncludeSelf is not set      add that as ServiceType
        ///    if more than one interface is found or IncludeSelf is set add ImplementationType as ServiceType; add found types(interfaces and classes) as ServiceType with a factory for ImplementationType
        /// </summary>
        IncludeSelfAndInterfaces = 3,

        /// <summary>
        /// 1) Add ServiceType if serviceType is not null
        /// 2) Scan for interfaces 
        /// 1) Scan for base classes (excluding object)
        /// 2) if one base class is found and IncludeSelf is not set      add that as ServiceType
        ///    if more than one base class is found or IncludeSelf is set add ImplementationType as ServiceType; add found types(classes) as ServiceType with a factory for ImplementationType
        /// </summary>
        IncludeClasses = 4,


        /// <summary>
        /// 1) Add ServiceType if serviceType is not null
        /// 2) Scan for interfaces and base types
        /// 3) add implementationType as ServiceType
        /// 4) add ServiceType for found types(interfaces and classes) as with a factory for ImplementationType
        /// </summary>
        IncludeSelfAndClasses = 5,

        /// <summary>
        /// 1) if ServiceType is not null throw an error
        ///    if ServiceType is null     Scan for base classes (not object) and interfaces
        /// 2) if one base class is found and IncludeSelf is not set      add that as ServiceType
        ///    if more than one base class is found or IncludeSelf is set add ImplementationType as ServiceType add found types(interfaces and classes) as ServiceType with a factory for ImplementationType
        /// </summary>
        IncludeClassesAndInterfaces = 6,

        IncludeAll = 7
    }

    /// <summary>
    /// Annotate class as service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ServiceDescriptorAttribute : Attribute {
        /// <summary>
        /// Register this class as a (Transient) service (with the default rules for serviceTypes).
        /// </summary>
        public ServiceDescriptorAttribute() {
            Mode = ServiceTypeMode.IncludeSelfAndInterfaces;
            ServiceType = null;
            Lifetime = ServiceLifetime.Transient;
        }

        /// <summary>
        /// Register this class as a (Transient) service with the given serviceType.
        /// </summary>
        /// <param name="serviceType">The serviceType is the the type that is used while resolving <see cref="IServiceProvider.GetService"/></param>
        public ServiceDescriptorAttribute(Type? serviceType) {
            ServiceType = serviceType;
            Lifetime = ServiceLifetime.Transient;
            Mode = ServiceTypeMode.IncludeSelfAndInterfaces;
        }

        /// <summary>
        /// Register this class as a service with the given serviceType and lifetime.
        /// </summary>
        /// <param name="serviceType">The serviceType is the the type that is used while resolving <see cref="IServiceProvider.GetService"/></param>
        /// <param name="lifetime">The lifeTime for the service instances.</param>
        public ServiceDescriptorAttribute(Type? serviceType, ServiceLifetime lifetime) {
            ServiceType = serviceType;
            Lifetime = lifetime;
            Mode = ServiceTypeMode.IncludeSelfAndInterfaces;
        }
        public ServiceDescriptorAttribute(Type? serviceType, ServiceLifetime lifetime, ServiceTypeMode mode) {
            ServiceType = serviceType;
            Lifetime = lifetime;
            Mode = mode;
        }

        /// <summary>
        /// Gets the <see cref="ServiceTypeMode"/>.
        /// </summary>
        public ServiceTypeMode Mode { get; init; }

        /// <summary>
        /// Register this ServiceType
        /// </summary>
        public Type? ServiceType { get; init; }

        public ServiceLifetime Lifetime { get; init; }
    }

    /// <summary>
    /// Annotate as singleton class by singleton lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SingletonAttribute : ServiceDescriptorAttribute {
        public SingletonAttribute() : base(null, ServiceLifetime.Singleton) {
        }
        public SingletonAttribute(Type? serviceType) : base(serviceType, ServiceLifetime.Singleton) {
        }
    }

    /// <summary>
    /// Annotate as scoped class by scoped lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ScopedAttribute : ServiceDescriptorAttribute {
        public ScopedAttribute() : base(null, ServiceLifetime.Scoped) {
        }
        public ScopedAttribute(Type? serviceType) : base(serviceType, ServiceLifetime.Scoped) {
        }
    }


    /// <summary>
    /// Annotate as transient class by transient lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TransientAttribute : ServiceDescriptorAttribute {
        public TransientAttribute() : base(null, ServiceLifetime.Transient) {
        }
        public TransientAttribute(Type? serviceType) : base(serviceType, ServiceLifetime.Transient) {
        }
    }
}
