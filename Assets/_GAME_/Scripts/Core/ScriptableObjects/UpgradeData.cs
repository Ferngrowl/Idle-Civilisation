using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an effect an upgrade has on the game
/// </summary>
[Serializable]
public class UpgradeEffect
{
    public EffectType Type;
    public string TargetID;
    public float Value;
}

/// <summary>
/// Types of effects an upgrade can have
/// </summary>
public enum EffectType
{
    ProductionMultiplier,
    BuildingProductionMultiplier,
    ConsumptionReduction,
    StorageMultiplier,
    UnlockBuilding,
    UnlockResource,
    UnlockUpgrade
}

/// <summary>
/// ScriptableObject containing upgrade configuration data
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/Upgrade Definition")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Identity")]
    [Tooltip("Unique identifier for the upgrade")]
    public string ID;
    
    [Tooltip("Display name shown in UI")]
    public string DisplayName;
    
    [Tooltip("Description of the upgrade")]
    [TextArea(2, 5)]
    public string Description;
    
    [Header("Upgrade Properties")]
    [Tooltip("Effects this upgrade has when purchased")]
    public List<UpgradeEffect> Effects = new List<UpgradeEffect>();
    
    [Tooltip("Resources required to purchase this upgrade")]
    public List<ResourceEffect> Costs = new List<ResourceEffect>();
    
    [Header("Visibility & Requirements")]
    [Tooltip("Whether this upgrade is visible from the start")]
    public bool VisibleByDefault = false;
    
    [Tooltip("Resources required to unlock this upgrade")]
    public List<string> RequiredResources = new List<string>();
    
    [Tooltip("Buildings required to unlock this upgrade")]
    public List<string> RequiredBuildings = new List<string>();
    
    [Tooltip("Other upgrades required to unlock this upgrade")]
    public List<string> RequiredUpgrades = new List<string>();
    
    [Header("UI Display")]
    [Tooltip("Icon for the upgrade")]
    public Sprite Icon;
    
    [Tooltip("Color for the upgrade in UI")]
    public Color Color = Color.white;
} 