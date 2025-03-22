using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all resources in the game. Handles production, consumption, and limits.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [SerializeField] private List<ResourceDefinition> resourceDefinitions = new List<ResourceDefinition>();
    
    // Runtime resource data
    private Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
    
    public void Initialize()
    {
        // Initialize all resources from definitions
        foreach (var definition in resourceDefinitions)
        {
            resources[definition.ID] = new Resource(definition);
        }
        
        // Register resource visibility conditions
        SetupResourceVisibility();
    }
    
    /// <summary>
    /// Setup advanced visibility conditions for resources
    /// </summary>
    private void SetupResourceVisibility()
    {
        // Example: Wood becomes visible after 10 catnip is collected
        if (resources.ContainsKey("wood") && resources.ContainsKey("catnip"))
        {
            resources["wood"].VisibilityCondition = () => GetAmount("catnip") >= 10f;
        }
    }
    
    public void Tick(float deltaTime)
    {
        // Calculate production for each resource
        foreach (var resource in resources.Values)
        {
            if (resource.IsUnlocked)
            {
                // Calculate production rate (from buildings, etc.)
                float productionRate = CalculateProductionRate(resource.Definition.ID);
                float consumptionRate = CalculateConsumptionRate(resource.Definition.ID);
                
                // Apply net change
                float netChange = (productionRate - consumptionRate) * deltaTime;
                AddResource(resource.Definition.ID, netChange);
            }
        }
    }
    
    /// <summary>
    /// Calculates total production rate for a resource from all sources
    /// </summary>
    private float CalculateProductionRate(string resourceID)
    {
        float baseProduction = 0f;
        
        // Get base production from buildings
        var buildings = GameManager.Instance.Buildings.GetAllBuildings();
        foreach (var building in buildings)
        {
            foreach (var production in building.Definition.Production)
            {
                if (production.ResourceID == resourceID)
                {
                    baseProduction += production.Amount * building.Count;
                }
            }
        }
        
        // Apply production multipliers (from upgrades, etc.)
        float multiplier = CalculateProductionMultiplier(resourceID);
        
        return baseProduction * multiplier;
    }
    
    /// <summary>
    /// Calculates total consumption rate for a resource from all sources
    /// </summary>
    private float CalculateConsumptionRate(string resourceID)
    {
        float consumption = 0f;
        
        // Get consumption from buildings
        var buildings = GameManager.Instance.Buildings.GetAllBuildings();
        foreach (var building in buildings)
        {
            foreach (var consumption in building.Definition.Consumption)
            {
                if (consumption.ResourceID == resourceID)
                {
                    consumption += consumption.Amount * building.Count;
                }
            }
        }
        
        return consumption;
    }
    
    /// <summary>
    /// Calculates production multiplier from all sources (upgrades, etc.)
    /// </summary>
    private float CalculateProductionMultiplier(string resourceID)
    {
        float multiplier = 1f;
        
        // Apply multipliers from upgrades
        var upgrades = GameManager.Instance.Upgrades.GetAllPurchasedUpgrades();
        foreach (var upgrade in upgrades)
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                if (effect.Type == EffectType.ProductionMultiplier && effect.TargetID == resourceID)
                {
                    multiplier *= effect.Value;
                }
            }
        }
        
        return multiplier;
    }
    
    public bool CanAfford(Dictionary<string, float> costs)
    {
        foreach (var cost in costs)
        {
            if (GetAmount(cost.Key) < cost.Value)
                return false;
        }
        return true;
    }
    
    public void SpendResources(Dictionary<string, float> costs)
    {
        if (!CanAfford(costs))
            return;
            
        foreach (var cost in costs)
        {
            AddResource(cost.Key, -cost.Value);
        }
    }
    
    public float GetAmount(string resourceID)
    {
        if (resources.ContainsKey(resourceID))
        {
            return resources[resourceID].Amount;
        }
        return 0f;
    }
    
    public float GetCapacity(string resourceID)
    {
        if (resources.ContainsKey(resourceID))
        {
            return resources[resourceID].Capacity;
        }
        return 0f;
    }
    
    public void AddResource(string resourceID, float amount)
    {
        if (resources.ContainsKey(resourceID))
        {
            Resource resource = resources[resourceID];
            
            // Check if adding would exceed capacity
            if (resource.Definition.HasCapacity)
            {
                float newAmount = Mathf.Min(resource.Amount + amount, resource.Capacity);
                resource.Amount = Mathf.Max(0, newAmount);
            }
            else
            {
                resource.Amount += amount;
            }
            
            // Resource was collected, mark as unlocked
            if (amount > 0 && !resource.IsUnlocked)
            {
                UnlockResource(resourceID);
            }
        }
    }
    
    public void SetCapacity(string resourceID, float capacity)
    {
        if (resources.ContainsKey(resourceID))
        {
            resources[resourceID].Capacity = capacity;
            
            // Cap current amount if it exceeds new capacity
            if (resources[resourceID].Amount > capacity)
            {
                resources[resourceID].Amount = capacity;
            }
        }
    }
    
    public void UnlockResource(string resourceID)
    {
        if (resources.ContainsKey(resourceID))
        {
            resources[resourceID].IsUnlocked = true;
            
            // Notify UI system about resource visibility change
            GameManager.Instance.UI.RefreshResourceView();
        }
    }
    
    public List<Resource> GetVisibleResources()
    {
        List<Resource> visibleResources = new List<Resource>();
        
        foreach (var resource in resources.Values)
        {
            if (resource.IsVisible && resource.IsUnlocked)
            {
                visibleResources.Add(resource);
            }
        }
        
        return visibleResources;
    }
    
    public void Reset()
    {
        // Reset all resources to initial state
        foreach (var definition in resourceDefinitions)
        {
            resources[definition.ID] = new Resource(definition);
        }
    }
    
    public ResourceData SerializeData()
    {
        ResourceData data = new ResourceData();
        data.resourceStates = new List<ResourceState>();
        
        foreach (var resource in resources.Values)
        {
            data.resourceStates.Add(new ResourceState
            {
                id = resource.Definition.ID,
                amount = resource.Amount,
                capacity = resource.Capacity,
                isUnlocked = resource.IsUnlocked
            });
        }
        
        return data;
    }
    
    public void DeserializeData(ResourceData data)
    {
        if (data == null || data.resourceStates == null)
            return;
            
        foreach (var state in data.resourceStates)
        {
            if (resources.ContainsKey(state.id))
            {
                resources[state.id].Amount = state.amount;
                resources[state.id].Capacity = state.capacity;
                resources[state.id].IsUnlocked = state.isUnlocked;
            }
        }
    }
}

/// <summary>
/// Definition for a resource type - used for editor configuration
/// </summary>
[Serializable]
public class ResourceDefinition
{
    public string ID;
    public string DisplayName;
    public string Description;
    public Sprite Icon;
    public bool HasCapacity = false;
    public float InitialCapacity = 100f;
    public float InitialAmount = 0f;
    public bool VisibleByDefault = false;
}

/// <summary>
/// Runtime instance of a resource
/// </summary>
public class Resource
{
    public ResourceDefinition Definition { get; private set; }
    public float Amount { get; set; }
    public float Capacity { get; set; }
    public bool IsUnlocked { get; set; }
    
    // Delegate for custom visibility conditions
    public Func<bool> VisibilityCondition { get; set; } = () => true;
    
    public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition());
    
    public Resource(ResourceDefinition definition)
    {
        Definition = definition;
        Amount = definition.InitialAmount;
        Capacity = definition.InitialCapacity;
        IsUnlocked = definition.VisibleByDefault;
    }
}

/// <summary>
/// Serializable data for resources
/// </summary>
[Serializable]
public class ResourceData
{
    public List<ResourceState> resourceStates;
}

/// <summary>
/// Serializable state of a single resource
/// </summary>
[Serializable]
public class ResourceState
{
    public string id;
    public float amount;
    public float capacity;
    public bool isUnlocked;
}