using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service locator pattern implementation to reduce direct dependencies 
/// and make the codebase more testable
/// </summary>
public class GameServices : MonoBehaviour
{
    public static GameServices Instance { get; private set; }
    
    // Dictionary to store services by type
    private Dictionary<Type, object> services = new Dictionary<Type, object>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Register a service for later retrieval
    /// </summary>
    /// <typeparam name="T">Type of service (typically an interface)</typeparam>
    /// <param name="service">Service implementation</param>
    public void RegisterService<T>(T service)
    {
        Type type = typeof(T);
        
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type.Name} is already registered! Overwriting...");
        }
        
        services[type] = service;
        Debug.Log($"Registered service: {type.Name}");
    }
    
    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <typeparam name="T">Type of service to unregister</typeparam>
    public void UnregisterService<T>()
    {
        Type type = typeof(T);
        
        if (!services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type.Name} is not registered!");
            return;
        }
        
        services.Remove(type);
        Debug.Log($"Unregistered service: {type.Name}");
    }
    
    /// <summary>
    /// Get a registered service
    /// </summary>
    /// <typeparam name="T">Type of service to retrieve</typeparam>
    /// <returns>The registered service, or default if not found</returns>
    public T GetService<T>() where T : class
    {
        Type type = typeof(T);
        
        if (!services.ContainsKey(type))
        {
            Debug.LogWarning($"Service of type {type.Name} is not registered!");
            return null;
        }
        
        return (T)services[type];
    }
    
    /// <summary>
    /// Try to get a registered service
    /// </summary>
    /// <typeparam name="T">Type of service to retrieve</typeparam>
    /// <param name="service">The service instance if found</param>
    /// <returns>True if service was found, false otherwise</returns>
    public bool TryGetService<T>(out T service) where T : class
    {
        Type type = typeof(T);
        
        if (!services.ContainsKey(type))
        {
            service = null;
            return false;
        }
        
        service = (T)services[type];
        return true;
    }
    
    /// <summary>
    /// Check if a service is registered
    /// </summary>
    /// <typeparam name="T">Type of service to check</typeparam>
    /// <returns>True if service is registered, false otherwise</returns>
    public bool HasService<T>()
    {
        return services.ContainsKey(typeof(T));
    }
}

/// <summary>
/// Extension methods to make accessing services more convenient
/// </summary>
public static class ServiceExtensions
{
    public static T GetService<T>(this MonoBehaviour component) where T : class
    {
        return GameServices.Instance.GetService<T>();
    }
    
    public static bool TryGetService<T>(this MonoBehaviour component, out T service) where T : class
    {
        return GameServices.Instance.TryGetService<T>(out service);
    }
    
    public static bool HasService<T>(this MonoBehaviour component)
    {
        return GameServices.Instance.HasService<T>();
    }
} 