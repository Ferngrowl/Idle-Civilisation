using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game.Interfaces;
using Game.Models;
using Serialization;
using GameConfiguration;

/// <summary>
/// Manages all buildings in the game. Handles construction, upgrades, and production.
/// </summary>
public class BuildingManager : MonoBehaviour, IBuildingManager
{
    [SerializeField] private List<GameConfiguration.BuildingDefinition> buildingDefinitions = new List<GameConfiguration.BuildingDefinition>();
    
    // Runtime building data
    private Dictionary<string, Game.Models.Building> buildings = new Dictionary<string, Game.Models.Building>();
    
    // Dependencies
    private IResourceManager resourceManager;
    private IUIManager uiManager;
    
    public void Initialize()
    {
        // Get dependencies
        resourceManager = ServiceLocator.Get<IResourceManager>();
        uiManager = ServiceLocator.Get<IUIManager>();
        
        // Initialize all buildings from definitions
        foreach (var definition in buildingDefinitions)
        {
            buildings[definition.ID] = new Game.Models.Building(definition);
        }
        
        // Set up visibility conditions
        SetupBuildingVisibility();
    }
    
    /// <summary>
    /// Setup advanced visibility conditions for buildings
    /// </summary>
    private void SetupBuildingVisibility()
    {
        foreach (var building in buildings.Values)
        {
            // Set visibility function based on required buildings
            if (building.Definition.RequiredBuildings != null && building.Definition.RequiredBuildings.Count > 0)
            {
                building.VisibilityCondition = () => 
                    building.Definition.RequiredBuildings.All(reqBuildingId =>
                        GetBuildingCount(reqBuildingId) > 0
                    );
            }
        }
    }
    
    public void Tick(long tickNumber)
    {
        // Update buildings if needed (e.g., construction progress)
        foreach (var building in buildings.Values)
        {
            if (building.IsUnlocked)
            {
                // Any per-tick logic for buildings
            }
        }
        
        // Update storage capacities
        UpdateResourceCapacities();
    }
    
    /// <summary>
    /// Updates storage capacities for all resources based on buildings
    /// </summary>
    private void UpdateResourceCapacities()
    {
        if (resourceManager == null) return;
        
        // Reset capacities
        Dictionary<string, float> capacities = new Dictionary<string, float>();
        
        // Calculate capacities from buildings
        foreach (var building in buildings.Values)
        {
            if (building.Count > 0)
            {
                foreach (var capacity in building.Definition.Capacity)
                {
                    if (!capacities.ContainsKey(capacity.ResourceID))
                    {
                        capacities[capacity.ResourceID] = 0f;
                    }
                    
                    capacities[capacity.ResourceID] += capacity.Amount * building.Count;
                }
            }
        }
        
        // Apply capacities
        foreach (var capacity in capacities)
        {
            resourceManager.SetCapacity(capacity.Key, capacity.Value);
        }
    }
    
    public bool CanConstructBuilding(string buildingID)
    {
        if (!buildings.ContainsKey(buildingID))
            return false;
            
        Game.Models.Building building = buildings[buildingID];
        
        // Check if building is visible and unlocked
        if (!building.IsVisible || !building.IsUnlocked)
            return false;
            
        // Calculate current cost
        Dictionary<string, float> currentCost = building.GetCurrentCost();
        
        // Check if we can afford it
        return resourceManager != null && resourceManager.CanAfford(currentCost);
    }
    
    public bool ConstructBuilding(string buildingID)
    {
        if (!CanConstructBuilding(buildingID))
            return false;
            
        // Calculate current cost
        Dictionary<string, float> currentCost = buildings[buildingID].GetCurrentCost();
        
        // Spend resources
        resourceManager.SpendResources(currentCost);
        
        // Increment building count
        buildings[buildingID].Count++;
        
        // Update UI
        if (uiManager != null)
            uiManager.RefreshBuildingView();
        
        // Update resource capacities
        UpdateResourceCapacities();
        
        return true;
    }
    
    public int GetBuildingCount(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID].Count;
        }
        return 0;
    }
    
    public Game.Models.Building GetBuilding(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID];
        }
        return null;
    }
    
    public List<Game.Models.Building> GetVisibleBuildings()
    {
        List<Game.Models.Building> visibleBuildings = new List<Game.Models.Building>();
        
        foreach (var building in buildings.Values)
        {
            if (building.IsVisible)
            {
                visibleBuildings.Add(building);
            }
        }
        
        return visibleBuildings;
    }
    
    public List<Game.Models.Building> GetAllBuildings()
    {
        return new List<Game.Models.Building>(buildings.Values);
    }
    
    private void UnlockBuilding(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            buildings[buildingID].IsUnlocked = true;
            
            // Notify UI system
            if (uiManager != null)
                uiManager.RefreshBuildingView();
        }
    }
    
    public void Reset()
    {
        // Reset all buildings to initial state
        foreach (var building in buildings.Values)
        {
            building.Reset();
        }
    }
    
    /// <summary>
    /// Create serializable data for save game
    /// </summary>
    public BuildingSaveData SerializeData()
    {
        BuildingSaveData data = new BuildingSaveData();
        
        foreach (var kvp in buildings)
        {
            data.Buildings.Add(new BuildingSaveData.BuildingData
            {
                ID = kvp.Key,
                Count = kvp.Value.Count,
                IsUnlocked = kvp.Value.IsUnlocked
            });
        }
        
        return data;
    }
    
    /// <summary>
    /// Load from serialized data
    /// </summary>
    public void DeserializeData(BuildingSaveData data)
    {
        if (data == null || data.Buildings == null)
            return;
            
        // Reset all buildings first
        foreach (var building in buildings.Values)
        {
            building.Reset();
        }
        
        // Apply saved values
        foreach (var savedBuilding in data.Buildings)
        {
            if (buildings.ContainsKey(savedBuilding.ID))
            {
                Game.Models.Building building = buildings[savedBuilding.ID];
                building.Count = savedBuilding.Count;
                building.IsUnlocked = savedBuilding.IsUnlocked;
            }
        }
        
        // Update capacities after loading
        UpdateResourceCapacities();
    }

    public GameConfiguration.BuildingDefinition GetBuildingDefinition(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID].Definition;
        }
        return null;
    }

    /// <summary>
    /// Calculate the cost to construct a building
    /// </summary>
    public Dictionary<string, float> CalculateBuildingCost(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID].GetCurrentCost();
        }
        return new Dictionary<string, float>();
    }
}

[Serializable]
public class BuildingVisibilityRequirement {
    public string RequiredBuildingID;
    public int RequiredCount = 1;
}

/// <summary>
/// Definition for a building type - used for editor configuration
/// </summary>
[Serializable]
public class BuildingDefinition
{
    public string ID;
    public string DisplayName;
    public string Description;
    public Sprite Icon;
    
    // Cost increases with each building constructed
    public List<ResourceAmount> BaseCost = new List<ResourceAmount>();
    public float CostScaling = 1.15f; // Default KG value
    
    // Resources this building produces per second
    public List<ResourceAmount> Production = new List<ResourceAmount>();
    
    // Resources this building consumes per second
    public List<ResourceAmount> Consumption = new List<ResourceAmount>();
    
    // Resource storage capacity this building provides
    public List<ResourceAmount> Capacity = new List<ResourceAmount>();

    public List<BuildingVisibilityRequirement> VisibilityRequirements = new List<BuildingVisibilityRequirement>();
    
    public bool VisibleByDefault = false;

    public List<string> RequiredBuildings = new List<string>();
}

/// <summary>
/// Runtime instance of a building
/// </summary>
public class Building
{
    public BuildingDefinition Definition { get; private set; }
    public int Count { get; set; }
    public bool IsUnlocked { get; set; }
    
    // Delegate for custom visibility conditions
    public Func<bool> VisibilityCondition { get; set; } = () => true;
    
    public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition());
    
    public Building(BuildingDefinition definition)
    {
        Definition = definition;
        Count = 0;
        IsUnlocked = definition.VisibleByDefault;
    }

    public Dictionary<string, float> GetCurrentCost()
    {
        Dictionary<string, float> currentCost = new Dictionary<string, float>();
        
        foreach (var cost in Definition.BaseCost)
        {
            float scaledCost = cost.Amount * Mathf.Pow(Definition.CostScaling, Count);
            currentCost[cost.ResourceID] = Mathf.Ceil(scaledCost);
        }
        
        return currentCost;
    }

    public void Reset()
    {
        Count = 0;
        IsUnlocked = Definition.VisibleByDefault;
    }
}

/// <summary>
/// Represents an amount of a specific resource
/// </summary>
[Serializable]
public class ResourceAmount
{
    public string ResourceID;
    public float Amount;
}

/// <summary>
/// Serializable data for buildings
/// </summary>
[Serializable]
public class BuildingSaveData
{
    public List<BuildingData> Buildings = new List<BuildingData>();

    [Serializable]
    public class BuildingData
    {
        public string ID;
        public int Count;
        public bool IsUnlocked;
    }
}