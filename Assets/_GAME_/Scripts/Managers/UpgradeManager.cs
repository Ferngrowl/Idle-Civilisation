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
    [SerializeField] private List<GameConfiguration.UpgradeDefinition> upgradeDefinitions = new List<GameConfiguration.UpgradeDefinition>();
    
    // Runtime upgrade data
    private Dictionary<string, Game.Models.Upgrade> upgrades = new Dictionary<string, Game.Models.Upgrade>();
    
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
            upgrades[definition.ID] = new Game.Models.Upgrade(definition);
        }
        
        SetupUpgradeVisibility();
    }
    
    /// <summary>
    /// Configure visibility conditions for upgrades
    /// </summary>
    private void SetupUpgradeVisibility()
    {
        foreach (var upgrade in upgrades.Values)
        {
            // Closure to capture the current upgrade
            var currentDef = upgrade.Definition;
            
            // Create a complex visibility condition that checks buildings, upgrades and resources
            upgrade.VisibilityCondition = () =>
            {
                // Check required buildings
                foreach (var buildingID in currentDef.RequiredBuildings)
                {
                    if (buildingManager.GetBuildingCount(buildingID) <= 0)
                        return false;
                }
                
                // Check required upgrades (using RequiredUpgrades instead of Prerequisites)
                foreach (var upgradeID in currentDef.RequiredUpgrades)
                {
                    if (!HasPurchasedUpgrade(upgradeID))
                        return false;
                }
                
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
    /// Check if an upgrade has all prerequisites met
    /// </summary>
    private bool ArePrerequisitesMet(GameConfiguration.UpgradeDefinition upgradeDefinition)
    {
        // Check required upgrades (using RequiredUpgrades instead of Prerequisites)
        foreach (var prerequisiteID in upgradeDefinition.RequiredUpgrades)
        {
            if (!HasPurchasedUpgrade(prerequisiteID))
                return false;
        }
        
        // Check building requirements
        foreach (var buildingID in upgradeDefinition.RequiredBuildings)
        {
            if (buildingManager.GetBuildingCount(buildingID) <= 0)
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Apply effects of a purchased upgrade
    /// </summary>
    private void ApplyUpgradeEffects(string upgradeID)
    {
        if (!upgrades.ContainsKey(upgradeID))
            return;
            
        Game.Models.Upgrade upgrade = upgrades[upgradeID];
        
        foreach (var effect in upgrade.Definition.Effects)
        {
            // Convert to GameConfiguration.EffectType for comparison
            GameConfiguration.EffectType effectType = (GameConfiguration.EffectType)(int)effect.Type;
            
            // Apply different effects based on type
            switch (effectType)
            {
                case GameConfiguration.EffectType.SpecialUnlock:
                    // Handle unlocking special content based on TargetID
                    if (effect.TargetID.StartsWith("resource."))
                    {
                        string resourceID = effect.TargetID.Substring(9);
                        // Use UnlockResource through resourceManager
                        // Note: If UnlockResource isn't in IResourceManager, may need to cast
                        if (resourceManager is ResourceManager rm)
                        {
                            rm.UnlockResource(resourceID);
                        }
                    }
                    break;
                    
                // Add cases for other effect types as needed
                case GameConfiguration.EffectType.ProductionMultiplier:
                    // Production multipliers are applied in ResourceManager's calculation
                    break;
                    
                case GameConfiguration.EffectType.ConsumptionReduction:
                    // Consumption reductions are applied in ResourceManager's calculation
                    break;
            }
        }
    }

    /// <summary>
    /// Check if all prerequisites for an upgrade are met
    /// </summary>
    public bool CanPurchaseUpgrade(string upgradeID)
    {
        if (!upgrades.ContainsKey(upgradeID))
            return false;
            
        Game.Models.Upgrade upgrade = upgrades[upgradeID];
        
        // Already purchased?
        if (upgrade.IsPurchased)
            return false;
            
        // Check resources
        if (!resourceManager.CanAfford(upgrade.GetCost()))
            return false;
            
        // Check prerequisites (using RequiredUpgrades property)
        return ArePrerequisitesMet(upgrade.Definition);
    }
    
    /// <summary>
    /// Check if the player has purchased a specific upgrade
    /// </summary>
    public bool HasPurchasedUpgrade(string upgradeID)
    {
        return upgrades.ContainsKey(upgradeID) && upgrades[upgradeID].IsPurchased;
    }

    /// <summary>
    /// Apply the effects of purchasing an upgrade
    /// </summary>
    public bool PurchaseUpgrade(string upgradeID)
    {
        if (!CanPurchaseUpgrade(upgradeID))
            return false;
            
        // Spend resources
        resourceManager.SpendResources(upgrades[upgradeID].GetCost());
        
        // Mark as purchased
        upgrades[upgradeID].IsPurchased = true;
        
        // Apply upgrade effects
        ApplyUpgradeEffects(upgradeID);
        
        // Refresh UI
        ServiceLocator.Get<IUIManager>().RefreshUpgradeView();
        
        return true;
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
    public List<Game.Models.Upgrade> GetAllUpgrades()
    {
        return new List<Game.Models.Upgrade>(upgrades.Values);
    }
    
    /// <summary>
    /// Get all purchased upgrades
    /// </summary>
    public List<Game.Models.Upgrade> GetAllPurchasedUpgrades()
    {
        List<Game.Models.Upgrade> purchased = new List<Game.Models.Upgrade>();
        
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
    /// Get all visible upgrades (for interface compatibility)
    /// </summary>
    public List<Game.Models.Upgrade> GetVisibleUpgrades()
    {
        List<Game.Models.Upgrade> visible = new List<Game.Models.Upgrade>();
        
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.IsVisible)
            {
                visible.Add(upgrade);
            }
        }
        
        return visible;
    }
    
    /// <summary>
    /// Get upgrades that are available for purchase (visible and not purchased)
    /// </summary>
    public List<Game.Models.Upgrade> GetAvailableUpgrades()
    {
        List<Game.Models.Upgrade> available = new List<Game.Models.Upgrade>();
        
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.IsVisible && !upgrade.IsPurchased)
            {
                available.Add(upgrade);
            }
        }
        
        return available;
    }

    /// <summary>
    /// Get an upgrade definition by ID
    /// </summary>
    public GameConfiguration.UpgradeDefinition GetUpgradeDefinition(string upgradeID)
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
    public Serialization.UpgradeSaveData SerializeData()
    {
        Serialization.UpgradeSaveData data = new Serialization.UpgradeSaveData();
        
        foreach (var kvp in upgrades)
        {
            data.Upgrades.Add(new Serialization.UpgradeSaveData.UpgradeData
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
    public void DeserializeData(Serialization.UpgradeSaveData data)
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
                Game.Models.Upgrade upgrade = upgrades[savedUpgrade.ID];
                upgrade.IsPurchased = savedUpgrade.IsPurchased;
                upgrade.IsUnlocked = savedUpgrade.IsUnlocked;
            }
        }
    }
}

/// <summary>
/// Serializable data for upgrades
/// </summary>
[Serializable]
public class UpgradeSaveData
{
    public List<UpgradeData> Upgrades = new List<UpgradeData>();
    
    [Serializable]
    public class UpgradeData
    {
        public string ID;
        public bool IsPurchased;
        public bool IsUnlocked;
    }
}

[Serializable]
public class UpgradeVisibilityRequirement {
    public string RequiredBuildingID;
    public int RequiredCount = 1;
    public string RequiredResourceID;
    public float RequiredAmount;
}