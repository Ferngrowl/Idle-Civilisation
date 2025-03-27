using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages all buildings in the game. Handles construction, upgrades, and production.
/// </summary>
public class BuildingManager : MonoBehaviour
{

    [SerializeField] private List<BuildingDefinition> buildingDefinitions = new List<BuildingDefinition>();
    
    // Runtime building data
    private Dictionary<string, Building> buildings = new Dictionary<string, Building>();
    
    public void Initialize()
    {
        // Initialize all buildings from definitions
        foreach (var definition in buildingDefinitions)
        {
            buildings[definition.ID] = new Building(definition);
        }
        
        // Set up visibility conditions
        SetupBuildingVisibility();
    }
    
    public BuildingDefinition GetBuildingDefinition(string buildingID) {
    // Example: Retrieve from a predefined list or dictionary
    return buildingDefinitions.Find(def => def.ID == buildingID);
    }
    
    /// <summary>
    /// Setup advanced visibility conditions for buildings
    /// </summary>
    private void SetupBuildingVisibility()
    {
        foreach (var building in buildings.Values)
        {
            if (building.Definition.VisibilityRequirements != null && building.Definition.VisibilityRequirements.Count > 0)
            {
                building.VisibilityCondition = () => 
                    building.Definition.VisibilityRequirements.All(req =>
                        GetBuildingCount(req.RequiredBuildingID) >= req.RequiredCount
                    );
            }
        }
    }
    
    public void Tick(float deltaTime)
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
        ResourceManager resourceManager = GameManager.Instance.Resources;
        
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
            
        Building building = buildings[buildingID];
        
        // Check if building is visible and unlocked
        if (!building.IsVisible || !building.IsUnlocked)
            return false;
            
        // Calculate current cost
        Dictionary<string, float> currentCost = CalculateBuildingCost(buildingID);
        
        // Check if we can afford it
        return GameManager.Instance.Resources.CanAfford(currentCost);
    }
    
    public void ConstructBuilding(string buildingID)
    {
        if (!CanConstructBuilding(buildingID))
            return;
            
        // Calculate current cost
        Dictionary<string, float> currentCost = CalculateBuildingCost(buildingID);
        
        // Spend resources
        GameManager.Instance.Resources.SpendResources(currentCost);
        
        // Increment building count
        buildings[buildingID].Count++;
        
        // Update UI
        GameManager.Instance.UI.RefreshBuildingView();
        
        // Update resource capacities
        UpdateResourceCapacities();
    }
    
    /// <summary>
    /// Calculates the current cost of a building based on its base cost and scaling
    /// </summary>
    public Dictionary<string, float> CalculateBuildingCost(string buildingID)
    {
        if (!buildings.ContainsKey(buildingID))
            return new Dictionary<string, float>();
            
        Building building = buildings[buildingID];
        Dictionary<string, float> currentCost = new Dictionary<string, float>();
        
        foreach (var cost in building.Definition.BaseCost)
        {
            float scaledCost = cost.Amount * Mathf.Pow(building.Definition.CostScaling, building.Count);
            currentCost[cost.ResourceID] = Mathf.Ceil(scaledCost);
        }
        
        return currentCost;
    }
    
    public int GetBuildingCount(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID].Count;
        }
        return 0;
    }
    
    public Building GetBuilding(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID];
        }
        return null;
    }
    
    public List<Building> GetVisibleBuildings()
    {
        List<Building> visibleBuildings = new List<Building>();
        
        foreach (var building in buildings.Values)
        {
            if (building.IsVisible)
            {
                visibleBuildings.Add(building);
            }
        }
        
        return visibleBuildings;
    }
    
    public List<Building> GetAllBuildings()
    {
        return new List<Building>(buildings.Values);
    }
    
    public void UnlockBuilding(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            buildings[buildingID].IsUnlocked = true;
            
            // Notify UI system
            GameManager.Instance.UI.RefreshBuildingView();
        }
    }
    
    public void Reset()
    {
        // Reset all buildings to initial state
        foreach (var definition in buildingDefinitions)
        {
            buildings[definition.ID] = new Building(definition);
        }
    }
    
    public BuildingData SerializeData()
    {
        BuildingData data = new BuildingData();
        data.buildingStates = new List<BuildingState>();
        
        foreach (var building in buildings.Values)
        {
            data.buildingStates.Add(new BuildingState
            {
                id = building.Definition.ID,
                count = building.Count,
                isUnlocked = building.IsUnlocked
            });
        }
        
        return data;
    }
    
    public void DeserializeData(BuildingData data)
    {
        if (data == null || data.buildingStates == null)
            return;
            
        foreach (var state in data.buildingStates)
        {
            if (buildings.ContainsKey(state.id))
            {
                buildings[state.id].Count = state.count;
                buildings[state.id].IsUnlocked = state.isUnlocked;
            }
        }
        
        // Update capacities after loading
        UpdateResourceCapacities();
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
public class BuildingData
{
    public List<BuildingState> buildingStates;
}

/// <summary>
/// Serializable state of a single building
/// </summary>
[Serializable]
public class BuildingState
{
    public string id;
    public int count;
    public bool isUnlocked;
}