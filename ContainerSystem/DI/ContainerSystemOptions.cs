/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

namespace ContainerSystem.DI;

/// <summary>
/// Configuration options for the container system.
/// Used to configure behavior when registering services via dependency injection.
/// </summary>
public class ContainerSystemOptions
{
    /// <summary>
    /// Gets or sets whether thread safety should be enabled by default for new containers.
    /// Default is false for better single-threaded performance.
    /// </summary>
    public bool EnableThreadSafetyByDefault { get; set; } = false;
}
