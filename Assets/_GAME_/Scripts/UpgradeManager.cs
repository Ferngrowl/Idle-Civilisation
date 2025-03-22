using System;
using System.Collections.Generic;
using UnityEngine;

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
    
    /// <summary>
    /// Setup advanced visibility conditions for upgrades
    /// </summary>
    private void SetupUpgradeVisibility()
    {
        // Example: Agriculture upgrade becomes visible after you have 5 catnip fields
        if (upgrades.ContainsKey("agriculture") && GameManager.Instance.Buildings != null)
        {
            upgrades["agriculture"].VisibilityCondition = () => 
                GameManager.Instance.Buildings.GetBuildingCount("catnipField") >= 5;
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
        bool visibilityChanged = false;
        
        foreach (var upgrade in upgrades.Values)
        {
            bool wasVisible = upgrade.IsVisible;
            // Visibility is recalculated when the property is accessed
            if (upgrade.IsVisible != wasVisible)
            {
                visibilityChanged = true;
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
        
        // Check if upgrade is visible, unlocked, and not already purchased
        if (!upgrade.IsVisible || !upgrade.IsUnlocked || upgrade.IsPurchased)
            return false;
            
        // Check if prerequisites are met
        foreach (var prerequisite in upgrade.Definition.Prerequisites)
        {
            if (!IsUpgradePurchased(prerequisite))
                return false;
        }
        
        // Check if we can afford it
        return GameManager.Instance.Resources.CanAfford(upgrade.Definition.Cost);
    }
    
    public void PurchaseUpgrade(string upgradeID)
    {
        if (!CanPurchaseUpgrade(upgradeID))
            return;
            
        Upgrade upgrade = upgrades[upgradeID];
        
        // Spend resources
        GameManager.Instance.Resources.SpendResources(upgrade.Definition.Cost);
        
        // Mark as purchased
        upgrade.IsPurchased = true;
        
        // Apply immediate effects
        ApplyUpgradeEffects(upgrade);
        
        // Update UI
        GameManager.Instance.UI.RefreshUpgradeView();
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
                    GameManager.Instance.Buildings.UnlockBuilding(effect.TargetID);
                    break;
                    
                case EffectType.UnlockUpgrade:
                    UnlockUpgrade(effect.TargetID);
                    break;
                    
                case EffectType.UnlockResource:
                    GameManager.Instance.Resources.UnlockResource(effect.TargetID);
                    break;
                    
                case EffectType.ProductionMultiplier:
                    // Applied in resource production calculation
                    break;
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
            if (upgrade.IsVisible && !upgrade.IsPurchased)
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
    ProductionMultiplier
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
    
    // Cost to purchase the upgrade
    public Dictionary<string, float> Cost = new Dictionary<string, float>();
    
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
    
    public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition()) && !IsPurchased;
    
    public Upgrade(UpgradeDefinition definition)
    {
        Definition = definition;
        IsPurchased = false;
        IsUnlocked = definition.Visible