using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service locator pattern implementation for dependency injection
/// Allows managers to access other managers without direct references
/// </summary>
public static class ServiceLocator
{
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
    
    /// <summary>
    /// Register a service implementation
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <param name="service">Implementation of the interface</param>
    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
        Debug.Log($"Service registered: {typeof(T).Name}");
    }
    
    /// <summary>
    /// Get a service implementation
    /// </summary>
    /// <typeparam name="T">Type of service to retrieve</typeparam>
    /// <returns>The service implementation or null if not found</returns>
    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        
        Debug.LogWarning($"Service not found: {typeof(T).Name}");
        return null;
    }
    
    /// <summary>
    /// Clear all registered services
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
    }
} 