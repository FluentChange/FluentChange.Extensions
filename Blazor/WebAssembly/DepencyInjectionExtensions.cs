
using Blazored.LocalStorage;
using FluentChange.Blazor.Interfaces;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace FluentChange.Blazor.WebAssembly
{
    public static class DepencyInjectionExtensions
    {
        public static LazyBlazorPluginBuilder UseLazyBlazorPlugins(this WebAssemblyHostBuilder builder)
        {
            // create own LightInject container so that it can be injected and manipulated later. used for late loading services
            var container = new ServiceContainer();
            container.RegisterInstance(container);

            // use LightInject
            builder.ConfigureContainer(new LightInjectServiceProviderFactory(container));


            var thisAssembly = Assembly.GetExecutingAssembly();
            var callingAssembly = Assembly.GetCallingAssembly();

            // auto register by interface
            builder.Services.AddAllInterfaceServices(thisAssembly); // load all services from this assembly
            builder.Services.AddAllInterfaceServices(callingAssembly); // load all services from executing assembly

            builder.Services.AddBlazoredLocalStorage(); // used to cache lazy loaded assemblies locally, for fast startup

            var resolver = new LazyAssemblyWebResolver();
            var lazyBuilder = new LazyBlazorPluginBuilder(resolver);

            container.RegisterInstance(resolver);

            return lazyBuilder;

        }

        public static void AddAllInterfaceServices(this IServiceCollection services, Assembly assembly = null)
        {
            if (assembly == null) assembly = Assembly.GetExecutingAssembly();

            services.AddInterfaceServices<ISingletonService>(ServiceLifetime.Singleton, assembly);
            services.AddInterfaceServices<IScopedService>(ServiceLifetime.Scoped, assembly);
            services.AddInterfaceServices<ITransientService>(ServiceLifetime.Transient, assembly);
        }

        public static void AddAllInterfaceServices(this ServiceContainer container, Assembly assembly = null)
        {
            if (assembly == null) assembly = Assembly.GetExecutingAssembly();

            container.AddInterfaceServices<ISingletonService>(ServiceLifetime.Singleton, assembly);
            container.AddInterfaceServices<IScopedService>(ServiceLifetime.Scoped, assembly);
            container.AddInterfaceServices<ITransientService>(ServiceLifetime.Transient, assembly);
        }

        public static void AddInterfaceServices<T>(this IServiceCollection services, ServiceLifetime lifetime, Assembly assembly = null)
        {
            var serviceTypes = assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)));

            foreach (var type in serviceTypes)
            {
                switch (lifetime)
                {
                    case ServiceLifetime.Scoped: services.AddScoped(type); break;
                    case ServiceLifetime.Singleton: services.AddSingleton(type); break;
                    case ServiceLifetime.Transient: services.AddTransient(type); break;
                }

            }

        }
        public static void AddInterfaceServices<T>(this ServiceContainer container, ServiceLifetime lifetime, Assembly assembly = null)
        {
            var serviceTypes = assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)));

            foreach (var type in serviceTypes)
            {
                var lightInjectLifetime = ResolveLifetime(lifetime);
                container.Register(type, lightInjectLifetime);
            }

        }

        public static void AddFrom(this ServiceContainer container, IServiceCollection services)
        {
            foreach (var service in services)
            {
                container.AddFrom(service);
            }
        }

        private static void AddFrom(this ServiceContainer container, ServiceDescriptor descriptor)
        {
            var registration = CreateServiceRegistration(descriptor);
            container.Register(registration);
        }



        private static ILifetime ResolveLifetime(ServiceLifetime serviceLifetime)
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Scoped: return new PerScopeLifetime();
                case ServiceLifetime.Singleton: return new PerContainerLifetime();
                case ServiceLifetime.Transient: return new PerRequestLifeTime();
                default: throw new ArgumentException();
            }
        }



        private static ServiceRegistration CreateServiceRegistration(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationFactory != null)
            {
                return CreateServiceRegistrationForFactoryDelegate(serviceDescriptor);
            }

            if (serviceDescriptor.ImplementationInstance != null)
            {
                return CreateServiceRegistrationForInstance(serviceDescriptor);
            }

            return CreateServiceRegistrationServiceType(serviceDescriptor);
        }

        private static ServiceRegistration CreateServiceRegistrationServiceType(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor);
            registration.ImplementingType = serviceDescriptor.ImplementationType;
            return registration;
        }

        private static ServiceRegistration CreateServiceRegistrationForInstance(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor);
            registration.Value = serviceDescriptor.ImplementationInstance;
            return registration;
        }

        private static ServiceRegistration CreateServiceRegistrationForFactoryDelegate(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor);
            registration.FactoryExpression = CreateFactoryDelegate(serviceDescriptor);
            return registration;
        }

        private static ServiceRegistration CreateBasicServiceRegistration(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = new ServiceRegistration
            {
                Lifetime = ResolveLifetime(serviceDescriptor.Lifetime),
                ServiceType = serviceDescriptor.ServiceType,
                ServiceName = Guid.NewGuid().ToString(),
            };
            return registration;
        }





        private static Delegate CreateFactoryDelegate(ServiceDescriptor serviceDescriptor)
        {
            var openGenericMethod = typeof(DependencyInjectionContainerExtensions).GetTypeInfo().GetDeclaredMethod("CreateTypedFactoryDelegate");
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(serviceDescriptor.ServiceType);
            return (Delegate)closedGenericMethod.Invoke(null, new object[] { serviceDescriptor });
        }

#pragma warning disable IDE0051
        private static Func<IServiceFactory, T> CreateTypedFactoryDelegate<T>(ServiceDescriptor serviceDescriptor)
            => serviceFactory => (T)serviceDescriptor.ImplementationFactory(new LightInjectServiceProvider(serviceFactory));
#pragma warning restore IDE0051

    }


    // copy from https://github.com/seesharper/LightInject.Microsoft.DependencyInjection/blob/master/src/LightInject.Microsoft.DependencyInjection/LightInject.Microsoft.DependencyInjection.cs
    internal class LightInjectServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
    {
        private readonly IServiceFactory serviceFactory;

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectServiceProvider"/> class.
        /// </summary>
        /// <param name="serviceFactory">The underlying <see cref="IServiceFactory"/>.</param>
        public LightInjectServiceProvider(IServiceFactory serviceFactory)
            => this.serviceFactory = serviceFactory;

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            if (serviceFactory is Scope scope)
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to return.</param>
        /// <returns>An instance of the given <paramref name="serviceType"/>.
        /// Throws an exception if it cannot be created.</returns>
        public object GetRequiredService(Type serviceType)
            => serviceFactory.GetInstance(serviceType);

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to return.</param>
        /// <returns>An instance of the given <paramref name="serviceType"/>.</returns>
        public object GetService(Type serviceType)
            => serviceFactory.TryGetInstance(serviceType);
    }


}
