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
/// Interface for the Resource Manager service
/// </summary>
public interface IResourceManager
{
    float GetAmount(string resourceID);
    float GetCapacity(string resourceID);
    void AddResource(string resourceID, float amount);
    bool CanAfford(Dictionary<string, float> costs);
    void SpendResources(Dictionary<string, float> costs);
    ResourceDefinition GetResourceDefinition(string id);
    List<Resource> GetVisibleResources();
    void UnlockResource(string resourceID);
}

/// <summary>
/// Interface for the Building Manager service
/// </summary>
public interface IBuildingManager
{
    int GetBuildingCount(string buildingID);
    Building GetBuilding(string buildingID);
    BuildingDefinition GetBuildingDefinition(string buildingID);
    List<Building> GetVisibleBuildings();
    List<Building> GetAllBuildings();
    bool CanConstructBuilding(string buildingID);
    void ConstructBuilding(string buildingID);
    Dictionary<string, float> CalculateBuildingCost(string buildingID);
    void UnlockBuilding(string buildingID);
}

/// <summary>
/// Interface for the Upgrade Manager service
/// </summary>
public interface IUpgradeManager
{
    bool IsUpgradePurchased(string upgradeID);
    Upgrade GetUpgrade(string upgradeID);
    UpgradeDefinition GetUpgradeDefinition(string upgradeID);
    List<Upgrade> GetVisibleUpgrades();
    List<Upgrade> GetAllPurchasedUpgrades();
    bool CanPurchaseUpgrade(string upgradeID);
    void PurchaseUpgrade(string upgradeID);
    void UnlockUpgrade(string upgradeID);
}

/// <summary>
/// Interface for the UI Manager service
/// </summary>
public interface IUIManager
{
    void RefreshResourceView();
    void RefreshBuildingView();
    void RefreshUpgradeView();
    void ShowTab(UITab tab);
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