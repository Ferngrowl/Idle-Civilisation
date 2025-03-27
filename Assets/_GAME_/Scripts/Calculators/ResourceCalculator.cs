using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all resource calculations including production, consumption, and capacity
/// </summary>
public class ResourceCalculator
{
    private ResourceManager resourceManager;
    private BuildingManager buildingManager;
    private UpgradeManager upgradeManager;
    
    public ResourceCalculator(ResourceManager resourceManager, BuildingManager buildingManager, UpgradeManager upgradeManager)
    {
        this.resourceManager = resourceManager;
        this.buildingManager = buildingManager;
        this.upgradeManager = upgradeManager;
    }
    
    /// <summary>
    /// Calculates total production rate for a resource from all sources
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Production rate per second</returns>
    public float CalculateProductionRate(string resourceID)
    {
        float baseProduction = CalculateBaseProduction(resourceID);
        float resourceMultiplier = CalculateResourceProductionMultiplier(resourceID);
        
        return baseProduction * resourceMultiplier;
    }
    
    /// <summary>
    /// Calculates base production without any multipliers
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Base production rate per second</returns>
    private float CalculateBaseProduction(string resourceID)
    {
        float baseProduction = 0f;
        
        // Get all buildings
        var buildings = buildingManager.GetAllBuildings();
        
        foreach (var building in buildings)
        {
            if (building.Count <= 0)
                continue;
                
            // Get building-specific production multiplier
            float buildingMultiplier = CalculateBuildingProductionMultiplier(building.Definition.ID);
            
            // Add production for this resource from this building
            foreach (var production in building.Definition.Production)
            {
                if (production.ResourceID == resourceID)
                {
                    baseProduction += production.Amount * building.Count * buildingMultiplier;
                }
            }
        }
        
        return baseProduction;
    }
    
    /// <summary>
    /// Calculates multiplier for a specific building's production
    /// </summary>
    /// <param name="buildingID">Building identifier</param>
    /// <returns>Production multiplier</returns>
    public float CalculateBuildingProductionMultiplier(string buildingID)
    {
        float multiplier = 1f;
        
        // Apply multipliers from purchased upgrades
        var upgrades = upgradeManager.GetAllPurchasedUpgrades();
        foreach (var upgrade in upgrades)
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                if (effect.Type == EffectType.BuildingProductionMultiplier && 
                    effect.TargetID == buildingID)
                {
                    multiplier *= effect.Value;
                }
            }
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Calculates resource-specific production multiplier from upgrades
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Production multiplier</returns>
    public float CalculateResourceProductionMultiplier(string resourceID)
    {
        float multiplier = 1f;
        
        // Apply multipliers from purchased upgrades
        var upgrades = upgradeManager.GetAllPurchasedUpgrades();
        foreach (var upgrade in upgrades)
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                if (effect.Type == EffectType.ProductionMultiplier && 
                    effect.TargetID == resourceID)
                {
                    multiplier *= effect.Value;
                }
            }
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Calculates consumption rate for a resource from all buildings
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Consumption rate per second</returns>
    public float CalculateConsumptionRate(string resourceID)
    {
        float baseConsumption = CalculateBaseConsumption(resourceID);
        float multiplier = CalculateConsumptionReductionMultiplier(resourceID);
        
        return baseConsumption * multiplier;
    }
    
    /// <summary>
    /// Calculates base consumption without any multipliers
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Base consumption rate per second</returns>
    private float CalculateBaseConsumption(string resourceID)
    {
        float baseConsumption = 0f;
        
        // Get all buildings
        var buildings = buildingManager.GetAllBuildings();
        
        foreach (var building in buildings)
        {
            if (building.Count <= 0)
                continue;
                
            // Add consumption for this resource from this building
            foreach (var consumption in building.Definition.Consumption)
            {
                if (consumption.ResourceID == resourceID)
                {
                    baseConsumption += consumption.Amount * building.Count;
                }
            }
        }
        
        return baseConsumption;
    }
    
    /// <summary>
    /// Calculates consumption reduction multiplier from upgrades
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Consumption multiplier (values less than 1 reduce consumption)</returns>
    public float CalculateConsumptionReductionMultiplier(string resourceID)
    {
        float multiplier = 1f;
        
        // Apply multipliers from purchased upgrades
        var upgrades = upgradeManager.GetAllPurchasedUpgrades();
        foreach (var upgrade in upgrades)
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                if (effect.Type == EffectType.ConsumptionReduction && 
                    effect.TargetID == resourceID)
                {
                    multiplier *= effect.Value;
                }
            }
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Calculates storage capacity for a resource
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Total storage capacity</returns>
    public float CalculateCapacity(string resourceID)
    {
        float baseCapacity = CalculateBaseCapacity(resourceID);
        float multiplier = CalculateStorageMultiplier(resourceID);
        
        return baseCapacity * multiplier;
    }
    
    /// <summary>
    /// Calculates base capacity without any multipliers
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Base storage capacity</returns>
    private float CalculateBaseCapacity(string resourceID)
    {
        float baseCapacity = 0f;
        
        // Get all buildings
        var buildings = buildingManager.GetAllBuildings();
        
        foreach (var building in buildings)
        {
            if (building.Count <= 0)
                continue;
                
            // Add capacity for this resource from this building
            foreach (var capacity in building.Definition.Capacity)
            {
                if (capacity.ResourceID == resourceID)
                {
                    baseCapacity += capacity.Amount * building.Count;
                }
            }
        }
        
        return baseCapacity;
    }
    
    /// <summary>
    /// Calculates storage multiplier from upgrades
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Storage multiplier</returns>
    public float CalculateStorageMultiplier(string resourceID)
    {
        float multiplier = 1f;
        
        // Apply multipliers from purchased upgrades
        var upgrades = upgradeManager.GetAllPurchasedUpgrades();
        foreach (var upgrade in upgrades)
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                if ((effect.Type == EffectType.StorageMultiplier || 
                     effect.Type == EffectType.ResourceCapacityMultiplier) && 
                    effect.TargetID == resourceID)
                {
                    multiplier *= effect.Value;
                }
            }
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Calculates net production rate (production - consumption)
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Net production rate per second</returns>
    public float CalculateNetRate(string resourceID)
    {
        float production = CalculateProductionRate(resourceID);
        float consumption = CalculateConsumptionRate(resourceID);
        
        return production - consumption;
    }
    
    /// <summary>
    /// Calculates time until resource reaches capacity
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Time in seconds until full, or -1 if never</returns>
    public float CalculateTimeUntilFull(string resourceID)
    {
        float netRate = CalculateNetRate(resourceID);
        
        // If not producing, or consuming more than producing, never fill
        if (netRate <= 0)
            return -1;
            
        float capacity = resourceManager.GetCapacity(resourceID);
        float current = resourceManager.GetAmount(resourceID);
        
        // Already full
        if (current >= capacity)
            return 0;
            
        return (capacity - current) / netRate;
    }
    
    /// <summary>
    /// Calculates time until resource is depleted
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <returns>Time in seconds until empty, or -1 if never</returns>
    public float CalculateTimeUntilEmpty(string resourceID)
    {
        float netRate = CalculateNetRate(resourceID);
        
        // If producing more than consuming, or not consuming, never empty
        if (netRate >= 0)
            return -1;
            
        float current = resourceManager.GetAmount(resourceID);
        
        // Already empty
        if (current <= 0)
            return 0;
            
        return current / -netRate;
    }
    
    /// <summary>
    /// Calculates offline progress gains for a resource
    /// </summary>
    /// <param name="resourceID">Resource identifier</param>
    /// <param name="timeOffline">Time offline in seconds</param>
    /// <returns>Amount of resource gained</returns>
    public float CalculateOfflineProgress(string resourceID, float timeOffline)
    {
        // Cap offline time if needed
        float cappedOfflineTime = Mathf.Min(timeOffline, 86400); // Max 24 hours
        
        float netRate = CalculateNetRate(resourceID);
        
        // No net production
        if (netRate <= 0)
            return 0;
            
        float capacity = resourceManager.GetCapacity(resourceID);
        float current = resourceManager.GetAmount(resourceID);
        
        // Calculate potential gain
        float potentialGain = netRate * cappedOfflineTime;
        
        // Cap at capacity
        if (current + potentialGain > capacity && capacity > 0)
        {
            return capacity - current;
        }
        
        return potentialGain;
    }
} 