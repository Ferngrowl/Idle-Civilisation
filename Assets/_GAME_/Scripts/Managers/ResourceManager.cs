using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;
using Game.Models;
using Serialization;
using GameConfiguration;

/// <summary>
/// Manages all resources in the game. Handles production, consumption, and limits.
/// </summary>
public class ResourceManager : MonoBehaviour, IResourceManager
{
    [SerializeField] private List<GameConfiguration.ResourceDefinition> resourceDefinitions = new List<GameConfiguration.ResourceDefinition>();
    
    // Runtime resource data
    private Dictionary<string, Game.Models.Resource> resources = new Dictionary<string, Game.Models.Resource>();
    
    // Dependencies
    private IBuildingManager buildingManager;
    private IUpgradeManager upgradeManager;
    private ITimeManager timeManager;
    
    public float GetProductionRate(string resourceID) => CalculateProductionRate(resourceID);
    public float GetConsumptionRate(string resourceID) => CalculateConsumptionRate(resourceID);

    // Use the normal method with the correct return type
    public GameConfiguration.ResourceDefinition GetResourceDefinition(string id)
        => resourceDefinitions.Find(r => r.ID == id);
    
    public void Initialize()
    {
        // Get dependencies from ServiceLocator
        buildingManager = ServiceLocator.Get<IBuildingManager>();
        upgradeManager = ServiceLocator.Get<IUpgradeManager>();
        timeManager = ServiceLocator.Get<ITimeManager>();
        
        // Initialize all resources from definitions
        foreach (var definition in resourceDefinitions)
        {
            resources[definition.ID] = new Game.Models.Resource(definition);
        }
        
        // Setup any initial resource states or custom visibility logic here if needed
    }
    
    public void Tick(long tickNumber)
    {
        // Calculate production for each resource - process one tick's worth
        float deltaTime = 0.2f; // 5 ticks per second = 0.2 seconds per tick
        
        foreach (var resource in resources.Values)
        {
            if (resource.IsUnlocked)
            {
                // Calculate production rate (from buildings, etc.)
                float productionRate = CalculateProductionRate(resource.Definition.ID);
                float consumptionRate = CalculateConsumptionRate(resource.Definition.ID);
                
                // Apply seasonal effects if appropriate
                if (timeManager != null)
                {
                    float seasonalModifier = timeManager.GetSeasonalModifier(resource.Definition.ID);
                    productionRate *= seasonalModifier;
                }
                
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
        
        // Add production from buildings
        foreach (var building in buildingManager.GetAllBuildings())
        {
            if (building.Count <= 0) continue;
            
            foreach (var production in building.Definition.Production)
            {
                if (production.ResourceID == resourceID)
                {
                    baseProduction += production.Amount * building.Count;
                }
            }
        }
        
        // Apply upgrades
        float multiplier = 1f;
        foreach (var upgrade in upgradeManager.GetAllPurchasedUpgrades())
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                // Fully qualify the enum and properties with GameConfiguration namespace
                if ((int)effect.Type == (int)GameConfiguration.EffectType.ProductionMultiplier && 
                    effect.TargetID == resourceID)
                {
                    multiplier *= (1f + effect.Value);
                }
            }
        }
        
        return baseProduction * multiplier;
    }
    
    /// <summary>
    /// Calculates total consumption rate for a resource from all sources
    /// </summary>
    private float CalculateConsumptionRate(string resourceID)
    {
        float baseConsumption = 0f;
        
        // Add consumption from buildings
        foreach (var building in buildingManager.GetAllBuildings())
        {
            if (building.Count <= 0) continue;
            
            foreach (var consumption in building.Definition.Consumption)
            {
                if (consumption.ResourceID == resourceID)
                {
                    baseConsumption += consumption.Amount * building.Count;
                }
            }
        }
        
        // Apply upgrades
        float multiplier = 1f;
        foreach (var upgrade in upgradeManager.GetAllPurchasedUpgrades())
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                // Fully qualify the enum and properties with GameConfiguration namespace
                if ((int)effect.Type == (int)GameConfiguration.EffectType.ConsumptionReduction && 
                    effect.TargetID == resourceID)
                {
                    multiplier *= (1f + effect.Value);
                }
            }
        }
        
        return baseConsumption * multiplier;
    }
    
    /// <summary>
    /// Get current amount of a resource
    /// </summary>
    public float GetAmount(string resourceID)
    {
        if (resources.ContainsKey(resourceID))
        {
            return resources[resourceID].Amount;
        }
        return 0f;
    }
    
    /// <summary>
    /// Get current capacity of a resource
    /// </summary>
    public float GetCapacity(string resourceID)
    {
        if (resources.ContainsKey(resourceID))
        {
            return resources[resourceID].Capacity;
        }
        return 0f;
    }
    
    /// <summary>
    /// Add an amount of a resource
    /// </summary>
    public void AddResource(string resourceID, float amount)
    {
        if (resources.ContainsKey(resourceID))
        {
            Game.Models.Resource resource = resources[resourceID];
            
            // Check if adding would exceed capacity
            if (resource.Definition.HasCapacity)
            {
                resource.Amount = Mathf.Clamp(resource.Amount + amount, 0f, resource.Capacity);
            }
            else
            {
                resource.Amount = Mathf.Max(0f, resource.Amount + amount);
            }
        }
    }
    
    /// <summary>
    /// Set the capacity of a resource
    /// </summary>
    public void SetCapacity(string resourceID, float capacity)
    {
        if (resources.ContainsKey(resourceID))
        {
            Game.Models.Resource resource = resources[resourceID];
            resource.Capacity = Mathf.Max(0f, capacity);
            
            // Clamp amount if it now exceeds new capacity
            if (resource.Amount > resource.Capacity)
            {
                resource.Amount = resource.Capacity;
            }
        }
    }
    
    /// <summary>
    /// Check if player can afford a cost
    /// </summary>
    public bool CanAfford(Dictionary<string, float> costs)
    {
        foreach (var cost in costs)
        {
            if (GetAmount(cost.Key) < cost.Value)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Spend resources (after checking CanAfford)
    /// </summary>
    public void SpendResources(Dictionary<string, float> costs)
    {
        foreach (var cost in costs)
        {
            AddResource(cost.Key, -cost.Value);
        }
    }
    
    /// <summary>
    /// Unlock a resource for the player to see
    /// </summary>
    public void UnlockResource(string resourceID)
    {
        if (resources.ContainsKey(resourceID))
        {
            resources[resourceID].IsUnlocked = true;
            
            // Notify UI system about resource visibility change
            ServiceLocator.Get<IUIManager>().RefreshResourceView();
        }
    }
    
    /// <summary>
    /// Create serializable data for save game
    /// </summary>
    public Serialization.ResourceSaveData SerializeData()
    {
        Serialization.ResourceSaveData data = new Serialization.ResourceSaveData();
        
        foreach (var kvp in resources)
        {
            data.Resources.Add(new Serialization.ResourceSaveData.ResourceData
            {
                ID = kvp.Key,
                Amount = kvp.Value.Amount,
                Capacity = kvp.Value.Capacity,
                IsUnlocked = kvp.Value.IsUnlocked
            });
        }
        
        return data;
    }
    
    /// <summary>
    /// Load from serialized data
    /// </summary>
    public void DeserializeData(Serialization.ResourceSaveData data)
    {
        if (data == null || data.Resources == null)
            return;
            
        // Reset all resources first
        foreach (var resource in resources.Values)
        {
            resource.Reset();
        }
        
        // Apply saved values
        foreach (var savedResource in data.Resources)
        {
            if (resources.ContainsKey(savedResource.ID))
            {
                Game.Models.Resource resource = resources[savedResource.ID];
                resource.Amount = savedResource.Amount;
                resource.Capacity = savedResource.Capacity;
                resource.IsUnlocked = savedResource.IsUnlocked;
            }
        }
    }
    
    /// <summary>
    /// Reset all resources to initial state
    /// </summary>
    public void Reset()
    {
        foreach (var resource in resources.Values)
        {
            resource.Reset();
        }
    }
    
    /// <summary>
    /// Returns a list of resources that are currently visible to the player
    /// </summary>
    public List<Game.Models.Resource> GetVisibleResources()
    {
        List<Game.Models.Resource> visibleResources = new List<Game.Models.Resource>();
        
        foreach (var resource in resources.Values)
        {
            if (resource.IsVisible)
            {
                visibleResources.Add(resource);
            }
        }
        
        return visibleResources;
    }
}

/// <summary>
/// Serializable data for resources
/// </summary>
[Serializable]
public class ResourceSaveData
{
    public List<ResourceData> Resources = new List<ResourceData>();

    public Dictionary<string, float> ResourceAmounts = new Dictionary<string, float>();
    public Dictionary<string, bool> ResourceUnlocked = new Dictionary<string, bool>();
    public Dictionary<string, float> ResourceCapacities = new Dictionary<string, float>();
    
    [Serializable]
    public class ResourceData
    {
        public string ID;
        public float Amount;
        public float Capacity;
        public bool IsUnlocked;
    }
}