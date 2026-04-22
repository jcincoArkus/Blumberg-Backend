using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Adapters.Telemetry;

/// <summary>
/// Extension methods for registering services with automatic span instrumentation
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly ProxyGenerator _proxyGenerator = new();

    /// <summary>
    /// Adds a scoped service with automatic span instrumentation for methods decorated with [Span]
    /// </summary>
    /// <typeparam name="TInterface">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    public static IServiceCollection AddScopedWithSpan<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<SpanInterceptor>();
        
        services.AddScoped<TImplementation>();
        services.AddScoped<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<SpanInterceptor>();
            return _proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Adds a transient service with automatic span instrumentation for methods decorated with [Span]
    /// </summary>
    /// <typeparam name="TInterface">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    public static IServiceCollection AddTransientWithSpan<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<SpanInterceptor>();
        
        services.AddTransient<TImplementation>();
        services.AddTransient<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<SpanInterceptor>();
            return _proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Adds a singleton service with automatic span instrumentation for methods decorated with [Span]
    /// </summary>
    /// <typeparam name="TInterface">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    public static IServiceCollection AddSingletonWithSpan<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<SpanInterceptor>();
        
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<SpanInterceptor>();
            return _proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Adds a scoped service with automatic span instrumentation for methods decorated with [Span]
    /// Uses a factory function to create the implementation
    /// </summary>
    /// <typeparam name="TInterface">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    /// <param name="implementationFactory">Factory function to create the implementation</param>
    public static IServiceCollection AddScopedWithSpan<TInterface, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> implementationFactory)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<SpanInterceptor>();
        
        services.AddScoped<TInterface>(provider =>
        {
            var implementation = implementationFactory(provider);
            var interceptor = provider.GetRequiredService<SpanInterceptor>();
            return _proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Adds a transient service with automatic span instrumentation for methods decorated with [Span]
    /// Uses a factory function to create the implementation
    /// </summary>
    /// <typeparam name="TInterface">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    /// <param name="implementationFactory">Factory function to create the implementation</param>
    public static IServiceCollection AddTransientWithSpan<TInterface, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> implementationFactory)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<SpanInterceptor>();
        
        services.AddTransient<TInterface>(provider =>
        {
            var implementation = implementationFactory(provider);
            var interceptor = provider.GetRequiredService<SpanInterceptor>();
            return _proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Adds a singleton service with automatic span instrumentation for methods decorated with [Span]
    /// Uses a factory function to create the implementation
    /// </summary>
    /// <typeparam name="TInterface">The interface type</typeparam>
    /// <typeparam name="TImplementation">The implementation type</typeparam>
    /// <param name="implementationFactory">Factory function to create the implementation</param>
    public static IServiceCollection AddSingletonWithSpan<TInterface, TImplementation>(
        this IServiceCollection services,
        Func<IServiceProvider, TImplementation> implementationFactory)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<SpanInterceptor>();
        
        services.AddSingleton<TInterface>(provider =>
        {
            var implementation = implementationFactory(provider);
            var interceptor = provider.GetRequiredService<SpanInterceptor>();
            return _proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }
}

