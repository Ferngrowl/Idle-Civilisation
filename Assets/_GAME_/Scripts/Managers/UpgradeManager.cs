using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages all upgrades in the game. Handles purchasing, effects, and unlocks.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private List<UpgradeDefinition> upgradeDefinitions = new List<UpgradeDefinition>();
    
    // Runtime upgrade data
    private Dictionary<string, Upgrade> upgrades = new Dictionary<string, Upgrade>();
    
    public void Initialize()
    {
        // Initialize all upgrades from definitions
        foreach (var definition in upgradeDefinitions)
        {
            upgrades[definition.ID] = new Upgrade(definition);
        }
        
        // Set up visibility conditions
        SetupUpgradeVisibility();
    }
    
    public UpgradeDefinition GetUpgradeDefinition(string upgradeID) {
    // Example: Retrieve from a predefined list or dictionary
    return upgradeDefinitions.Find(def => def.ID == upgradeID);
    }
    
    /// <summary>
    /// Setup advanced visibility conditions for upgrades
    /// </summary>
    private void SetupUpgradeVisibility()
    {
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.Definition.VisibilityRequirements != null && upgrade.Definition.VisibilityRequirements.Count > 0)
            {
                upgrade.VisibilityCondition = () => 
                    upgrade.Definition.VisibilityRequirements.All(req =>
                        (string.IsNullOrEmpty(req.RequiredBuildingID) || 
                        GameManager.Instance.Buildings.GetBuildingCount(req.RequiredBuildingID) >= req.RequiredCount) &&
                        (string.IsNullOrEmpty(req.RequiredResourceID) || 
                        GameManager.Instance.Resources.GetAmount(req.RequiredResourceID) >= req.RequiredAmount)
                    );
            }
        }
    }
    
    public void Tick(float deltaTime)
    {
        // Check for new visible upgrades
        CheckVisibilityUpdates();
    }
    
    /// <summary>
    /// Checks if any upgrades have become visible and updates the UI
    /// </summary>
    private void CheckVisibilityUpdates()
    {
        Dictionary<string, bool> oldVisibility = new Dictionary<string, bool>();
        
        // First, store all current visibility states
        foreach (var pair in upgrades)
        {
            oldVisibility[pair.Key] = pair.Value.IsVisible;
        }
        
        // Reset all visibility conditions to force re-evaluation
        SetupUpgradeVisibility();
        
        // Now check for changes
        bool visibilityChanged = false;
        foreach (var pair in upgrades)
        {
            bool wasVisible = oldVisibility[pair.Key];
            bool isNowVisible = pair.Value.IsVisible;
            
            if (isNowVisible != wasVisible)
            {
                visibilityChanged = true;
                break;
            }
        }
        
        if (visibilityChanged)
        {
            GameManager.Instance.UI.RefreshUpgradeView();
        }
    }
    
    public bool CanPurchaseUpgrade(string upgradeID)
    {
        if (!upgrades.ContainsKey(upgradeID))
            return false;
            
        Upgrade upgrade = upgrades[upgradeID];
        
        // Check if upgrade is available for purchase
        if (!upgrade.IsAvailableForPurchase)
            return false;
            
        // Check if prerequisites are met
        if (upgrade.Definition.Prerequisites != null)
        {
            foreach (var prerequisite in upgrade.Definition.Prerequisites)
            {
                if (!IsUpgradePurchased(prerequisite))
                    return false;
            }
        }
        
        // Check if we can afford it
        return GameManager.Instance.Resources.CanAfford(
            upgrade.Definition.Cost.ToDictionary(r => r.ResourceID, r => r.Amount)
        );
    }
    
    public void PurchaseUpgrade(string upgradeID)
    {
        if (!CanPurchaseUpgrade(upgradeID))
            return;
            
        Upgrade upgrade = upgrades[upgradeID];
        
        // Spend resources
        GameManager.Instance.Resources.SpendResources(
            upgrade.Definition.Cost.ToDictionary(r => r.ResourceID, r => r.Amount)
        );
        
        // Mark as purchased
        upgrade.IsPurchased = true;
        
        // Apply immediate effects
        ApplyUpgradeEffects(upgrade);
        
        // Update UI
        GameManager.Instance.UI.RefreshUpgradeView();
        
        // Check if this unlocks other upgrades
        CheckDependentUpgrades(upgradeID);
    }
    
    /// <summary>
    /// Applies effects of an upgrade
    /// </summary>
    private void ApplyUpgradeEffects(Upgrade upgrade)
    {
        foreach (var effect in upgrade.Definition.Effects)
        {
            switch (effect.Type)
            {
                case EffectType.UnlockBuilding:
                    if (!string.IsNullOrEmpty(effect.TargetID))
                        GameManager.Instance.Buildings.UnlockBuilding(effect.TargetID);
                    break;
                    
                case EffectType.UnlockUpgrade:
                    if (!string.IsNullOrEmpty(effect.TargetID))
                        UnlockUpgrade(effect.TargetID);
                    break;
                    
                case EffectType.UnlockResource:
                    if (!string.IsNullOrEmpty(effect.TargetID))
                        GameManager.Instance.Resources.UnlockResource(effect.TargetID);
                    break;
                    
                case EffectType.ProductionMultiplier:
                    // Applied in resource production calculation
                    break;
                    
                case EffectType.StorageMultiplier:
                    // Applied in resource capacity calculation
                    break;
                    
                case EffectType.ConsumptionReduction:
                    // Applied in resource consumption calculation
                    break;
                    
                case EffectType.BuildingProductionMultiplier:
                    // Applied in building production calculation
                    break;
                    
                case EffectType.ResourceCapacityMultiplier:
                    // Applied in resource capacity calculation
                    break;
            }
        }
    }
    
    /// <summary>
    /// Checks if any upgrades should be unlocked because their prerequisite was purchased
    /// </summary>
    private void CheckDependentUpgrades(string purchasedUpgradeID)
    {
        foreach (var upgrade in upgrades.Values)
        {
            if (!upgrade.IsUnlocked && 
                upgrade.Definition.Prerequisites != null && 
                upgrade.Definition.Prerequisites.Contains(purchasedUpgradeID))
            {
                // Check if all prerequisites are now met
                bool allPrerequisitesMet = true;
                foreach (var prerequisite in upgrade.Definition.Prerequisites)
                {
                    if (!IsUpgradePurchased(prerequisite))
                    {
                        allPrerequisitesMet = false;
                        break;
                    }
                }
                
                if (allPrerequisitesMet)
                {
                    UnlockUpgrade(upgrade.Definition.ID);
                }
            }
        }
    }
    
    public bool IsUpgradePurchased(string upgradeID)
    {
        if (upgrades.ContainsKey(upgradeID))
        {
            return upgrades[upgradeID].IsPurchased;
        }
        return false;
    }
    
    public void UnlockUpgrade(string upgradeID)
    {
        if (upgrades.ContainsKey(upgradeID))
        {
            upgrades[upgradeID].IsUnlocked = true;
            
            // Notify UI system
            GameManager.Instance.UI.RefreshUpgradeView();
        }
    }
    
    public List<Upgrade> GetVisibleUpgrades()
    {
        List<Upgrade> visibleUpgrades = new List<Upgrade>();
        
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.IsAvailableForPurchase)
            {
                visibleUpgrades.Add(upgrade);
            }
        }
        
        return visibleUpgrades;
    }
    
    public List<Upgrade> GetAllPurchasedUpgrades()
    {
        List<Upgrade> purchasedUpgrades = new List<Upgrade>();
        
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.IsPurchased)
            {
                purchasedUpgrades.Add(upgrade);
            }
        }
        
        return purchasedUpgrades;
    }
    
    public Upgrade GetUpgrade(string upgradeID)
    {
        if (upgrades.ContainsKey(upgradeID))
        {
            return upgrades[upgradeID];
        }
        return null;
    }
    
    public void Reset()
    {
        // Reset all upgrades to initial state
        foreach (var definition in upgradeDefinitions)
        {
            upgrades[definition.ID] = new Upgrade(definition);
        }
    }
    
    public UpgradeData SerializeData()
    {
        UpgradeData data = new UpgradeData();
        data.upgradeStates = new List<UpgradeState>();
        
        foreach (var upgrade in upgrades.Values)
        {
            data.upgradeStates.Add(new UpgradeState
            {
                id = upgrade.Definition.ID,
                isPurchased = upgrade.IsPurchased,
                isUnlocked = upgrade.IsUnlocked
            });
        }
        
        return data;
    }
    
    public void DeserializeData(UpgradeData data)
    {
        if (data == null || data.upgradeStates == null)
            return;
            
        foreach (var state in data.upgradeStates)
        {
            if (upgrades.ContainsKey(state.id))
            {
                upgrades[state.id].IsPurchased = state.isPurchased;
                upgrades[state.id].IsUnlocked = state.isUnlocked;
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