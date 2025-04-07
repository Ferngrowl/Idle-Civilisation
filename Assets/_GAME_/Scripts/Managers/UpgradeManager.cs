using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game.Interfaces;
using Game.Models;
using Serialization;
using GameConfiguration;

/// <summary>
/// Manages upgrades. Handles purchasing, effects, and visibility.
/// </summary>
public class UpgradeManager : MonoBehaviour, IUpgradeManager
{
    [SerializeField] private List<UpgradeDefinition> upgradeDefinitions = new List<UpgradeDefinition>();
    
    // Runtime upgrade data
    private Dictionary<string, Upgrade> upgrades = new Dictionary<string, Upgrade>();
    
    // Dependencies
    private IResourceManager resourceManager;
    private IBuildingManager buildingManager;
    private IUIManager uiManager;
    
    public void Initialize()
    {
        // Get dependencies
        resourceManager = ServiceLocator.Get<IResourceManager>();
        buildingManager = ServiceLocator.Get<IBuildingManager>();
        uiManager = ServiceLocator.Get<IUIManager>();
        
        // Initialize all upgrades from definitions
        foreach (var definition in upgradeDefinitions)
        {
            upgrades[definition.ID] = new Upgrade(definition);
        }
        
        SetupVisibilityConditions();
    }
    
    /// <summary>
    /// Configure visibility conditions for upgrades
    /// </summary>
    private void SetupVisibilityConditions()
    {
        foreach (var upgrade in upgrades.Values)
        {
            var def = upgrade.Definition;
            
            upgrade.VisibilityCondition = () => 
            {
                // Check building requirements
                foreach (string requiredBuildingID in def.RequiredBuildings)
                {
                    if (buildingManager.GetBuildingCount(requiredBuildingID) <= 0)
                        return false;
                }
                
                // Check upgrade prerequisites
                foreach (string prerequisiteID in def.Prerequisites)
                {
                    if (!HasUpgrade(prerequisiteID))
                        return false;
                }
                
                // If we reach here, all requirements are met
                return true;
            };
        }
    }
    
    public void Tick(long tickNumber)
    {
        // Check for upgrades that should be unlocked based on visibility conditions
        foreach (var upgrade in upgrades.Values)
        {
            if (!upgrade.IsUnlocked && upgrade.VisibilityCondition())
            {
                upgrade.IsUnlocked = true;
                
                // Refresh UI
                uiManager.RefreshUpgradeView();
            }
        }
    }
    
    /// <summary>
    /// Check if player can purchase an upgrade
    /// </summary>
    public bool CanPurchaseUpgrade(string upgradeID)
    {
        // Check if upgrade exists and is not already purchased
        if (!upgrades.ContainsKey(upgradeID) || upgrades[upgradeID].IsPurchased)
            return false;
            
        // Check if upgrade is visible/unlocked
        if (!upgrades[upgradeID].IsVisible)
            return false;
            
        // Check upgrade prerequisites
        foreach (string prerequisiteID in upgrades[upgradeID].Definition.Prerequisites)
        {
            if (!HasUpgrade(prerequisiteID))
                return false;
        }
        
        // Check if player can afford the upgrade
        Dictionary<string, float> cost = upgrades[upgradeID].GetCost();
        return resourceManager.CanAfford(cost);
    }
    
    /// <summary>
    /// Purchase an upgrade
    /// </summary>
    public bool PurchaseUpgrade(string upgradeID)
    {
        if (!CanPurchaseUpgrade(upgradeID))
            return false;
            
        // Spend resources
        Dictionary<string, float> cost = upgrades[upgradeID].GetCost();
        resourceManager.SpendResources(cost);
        
        // Mark as purchased
        upgrades[upgradeID].IsPurchased = true;
        
        // Apply upgrade effects
        ApplyUpgradeEffects(upgradeID);
        
        // Refresh UI
        uiManager.RefreshUpgradeView();
        
        return true;
    }
    
    /// <summary>
    /// Apply the effects of an upgrade
    /// </summary>
    private void ApplyUpgradeEffects(string upgradeID)
    {
        if (!upgrades.ContainsKey(upgradeID))
            return;
            
        Upgrade upgrade = upgrades[upgradeID];
        
        foreach (var effect in upgrade.Definition.Effects)
        {
            switch (effect.Type)
            {
                case EffectType.UnlockResource:
                    resourceManager.UnlockResource(effect.TargetID);
                    break;
                    
                case EffectType.UnlockBuilding:
                    // Handled by BuildingManager visibility conditions
                    break;
                    
                case EffectType.UnlockUpgrade:
                    // Unlock the target upgrade
                    if (upgrades.ContainsKey(effect.TargetID))
                    {
                        upgrades[effect.TargetID].IsUnlocked = true;
                    }
                    break;
                    
                // Other effects are applied during resource calculations
            }
        }
    }
    
    /// <summary>
    /// Check if player has purchased an upgrade
    /// </summary>
    public bool HasUpgrade(string upgradeID)
    {
        return upgrades.ContainsKey(upgradeID) && upgrades[upgradeID].IsPurchased;
    }
    
    /// <summary>
    /// Get all upgrades
    /// </summary>
    public List<Upgrade> GetAllUpgrades()
    {
        return new List<Upgrade>(upgrades.Values);
    }
    
    /// <summary>
    /// Get all purchased upgrades
    /// </summary>
    public List<Upgrade> GetAllPurchasedUpgrades()
    {
        List<Upgrade> purchased = new List<Upgrade>();
        
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.IsPurchased)
            {
                purchased.Add(upgrade);
            }
        }
        
        return purchased;
    }
    
    /// <summary>
    /// Get all visible upgrades
    /// </summary>
    public List<Upgrade> GetVisibleUpgrades()
    {
        List<Upgrade> visible = new List<Upgrade>();
        
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.IsVisible && !upgrade.IsPurchased)
            {
                visible.Add(upgrade);
            }
        }
        
        return visible;
    }

    /// <summary>
    /// Get an upgrade definition by ID
    /// </summary>
    public UpgradeDefinition GetUpgradeDefinition(string upgradeID)
    {
        if (upgrades.ContainsKey(upgradeID))
        {
            return upgrades[upgradeID].Definition;
        }
        return null;
    }

    /// <summary>
    /// Reset all upgrades to initial state
    /// </summary>
    public void Reset()
    {
        foreach (var upgrade in upgrades.Values)
        {
            upgrade.Reset();
        }
    }
    
    /// <summary>
    /// Create serializable data for save game
    /// </summary>
    public UpgradeSaveData SerializeData()
    {
        UpgradeSaveData data = new UpgradeSaveData();
        
        foreach (var kvp in upgrades)
        {
            data.Upgrades.Add(new UpgradeSaveData.UpgradeData
            {
                ID = kvp.Key,
                IsPurchased = kvp.Value.IsPurchased,
                IsUnlocked = kvp.Value.IsUnlocked
            });
        }
        
        return data;
    }
    
    /// <summary>
    /// Load from serialized data
    /// </summary>
    public void DeserializeData(UpgradeSaveData data)
    {
        if (data == null || data.Upgrades == null)
            return;
        
        // Reset all upgrades first
        foreach (var upgrade in upgrades.Values)
        {
            upgrade.Reset();
        }
        
        // Apply saved values
        foreach (var savedUpgrade in data.Upgrades)
        {
            if (upgrades.ContainsKey(savedUpgrade.ID))
            {
                Upgrade upgrade = upgrades[savedUpgrade.ID];
                upgrade.IsPurchased = savedUpgrade.IsPurchased;
                upgrade.IsUnlocked = savedUpgrade.IsUnlocked;
            }
        }
    }
}

/// <summary>
/// Types of effects that upgrades can have
/// </summary>
public enum EffectType
{
    UnlockBuilding,
    UnlockUpgrade,
    UnlockResource,
    ProductionMultiplier,
    StorageMultiplier,
    ConsumptionReduction,
    BuildingProductionMultiplier,
    ResourceCapacityMultiplier
}

/// <summary>
/// Definition for an upgrade - used for editor configuration
/// </summary>
[Serializable]
public class UpgradeDefinition
{
    public string ID;
    public string DisplayName;
    public string Description;
    public Sprite Icon;

    public List<UpgradeVisibilityRequirement> VisibilityRequirements = new List<UpgradeVisibilityRequirement>();
    
    // Cost to purchase the upgrade
    public List<ResourceAmount> Cost = new List<ResourceAmount>();
    
    // IDs of other upgrades that must be purchased first
    public List<string> Prerequisites = new List<string>();
    
    // Effects this upgrade applies when purchased
    public List<UpgradeEffect> Effects = new List<UpgradeEffect>();
    
    public bool VisibleByDefault = false;

    public List<string> RequiredBuildings = new List<string>();
    public List<string> RequiredUpgrades = new List<string>();
}

/// <summary>
/// An effect applied by an upgrade
/// </summary>
[Serializable]
public class UpgradeEffect
{
    public EffectType Type;
    public string TargetID; // Resource, building, or upgrade ID
    public float Value;     // Multiplier or other value
}

/// <summary>
/// Runtime instance of an upgrade
/// </summary>
public class Upgrade
{
    public UpgradeDefinition Definition { get; private set; }
    public bool IsPurchased { get; set; }
    public bool IsUnlocked { get; set; }
    
    // Delegate for custom visibility conditions
    public Func<bool> VisibilityCondition { get; set; } = () => true;
    
    // Modified to separate showing purchased upgrades from showing available upgrades
    public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition());
    
    // Use this to check if the upgrade should be shown in the available upgrades list
    public bool IsAvailableForPurchase => IsVisible && !IsPurchased;
    
    public Upgrade(UpgradeDefinition definition)
    {
        Definition = definition;
        IsPurchased = false;
        IsUnlocked = definition.VisibleByDefault;
    }

    public Dictionary<string, float> GetCost()
    {
        Dictionary<string, float> cost = new Dictionary<string, float>();
        foreach (var resourceAmount in Definition.Cost)
        {
            cost[resourceAmount.ResourceID] = resourceAmount.Amount;
        }
        return cost;
    }

    public void Reset()
    {
        IsPurchased = false;
        IsUnlocked = Definition.VisibleByDefault;
    }
}

/// <summary>
/// Serializable data for upgrades
/// </summary>
[Serializable]
public class UpgradeData
{
    public List<UpgradeState> upgradeStates;
}

/// <summary>
/// Serializable state of a single upgrade
/// </summary>
[Serializable]
public class UpgradeState
{
    public string id;
    public bool isPurchased;
    public bool isUnlocked;
}

[Serializable]
public class UpgradeVisibilityRequirement {
    public string RequiredBuildingID;
    public int RequiredCount = 1;
    public string RequiredResourceID;
    public float RequiredAmount;
}