/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContainerSystem.DI;

/// <summary>
/// Extension methods for configuring container system services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds container system services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
    /// <remarks>
    /// This method registers the following services:
    /// <list type="bullet">
    ///   <item><see cref="IValueContainerFactory"/> as singleton</item>
    ///   <item><see cref="IWireProtocolSerializer"/> as singleton</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Startup.cs or Program.cs
    /// services.AddContainerSystem();
    ///
    /// // Usage via DI
    /// public class MyService
    /// {
    ///     private readonly IValueContainerFactory _factory;
    ///
    ///     public MyService(IValueContainerFactory factory)
    ///     {
    ///         _factory = factory;
    ///     }
    ///
    ///     public ValueContainer CreateMessage()
    ///     {
    ///         return _factory.Create("my_message");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddContainerSystem(this IServiceCollection services)
    {
        return services.AddContainerSystem(_ => { });
    }

    /// <summary>
    /// Adds container system services to the specified <see cref="IServiceCollection"/> with configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="configure">An action to configure the <see cref="ContainerSystemOptions"/></param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained</returns>
    /// <example>
    /// <code>
    /// // Enable thread safety by default
    /// services.AddContainerSystem(options =>
    /// {
    ///     options.EnableThreadSafetyByDefault = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddContainerSystem(
        this IServiceCollection services,
        Action<ContainerSystemOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ContainerSystemOptions();
        configure(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IValueContainerFactory>(sp =>
        {
            var opts = sp.GetRequiredService<ContainerSystemOptions>();
            return new ValueContainerFactory(opts);
        });
        services.TryAddSingleton<IWireProtocolSerializer, WireProtocolSerializer>();

        return services;
    }
}
