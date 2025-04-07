using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConfiguration
{
    /// <summary>
    /// Type of effect that an upgrade can have
    /// </summary>
    public enum EffectType
    {
        ProductionMultiplier,
        ConsumptionReduction,
        StorageMultiplier,
        ResourceCapacityMultiplier,
        SpecialUnlock
    }
    
    /// <summary>
    /// Represents a specific effect of an upgrade
    /// </summary>
    [Serializable]
    public class UpgradeEffect
    {
        [Tooltip("Type of effect this upgrade provides")]
        public EffectType Type;
        
        [Tooltip("Resource ID or other identifier this effect targets")]
        public string TargetID;
        
        [Tooltip("Value of the effect (multiplier, reduction factor, etc.)")]
        public float Value = 1.0f;
        
        [Tooltip("Description of this effect for UI display")]
        public string DisplayText;
    }
    
    /// <summary>
    /// Scriptable object for defining upgrades
    /// </summary>
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Upgrade Identity")]
        [Tooltip("Unique identifier for the upgrade")]
        public string ID;
        
        [Tooltip("Display name shown in UI")]
        public string DisplayName;
        
        [Tooltip("Description of the upgrade")]
        [TextArea(2, 5)]
        public string Description;
        
        [Header("Requirements")]
        [Tooltip("Whether this upgrade is visible from the start")]
        public bool VisibleByDefault = false;
        
        [Tooltip("Resources required to purchase this upgrade")]
        public List<ResourceAmount> Cost = new List<ResourceAmount>();
        
        [Tooltip("Required buildings to unlock this upgrade")]
        public List<string> RequiredBuildings = new List<string>();
        
        [Tooltip("Required upgrades to unlock this upgrade")]
        public List<string> RequiredUpgrades = new List<string>();
        
        [Header("Effects")]
        [Tooltip("Effects this upgrade provides when purchased")]
        public List<UpgradeEffect> Effects = new List<UpgradeEffect>();
        
        [Header("UI")]
        [Tooltip("Icon for the upgrade")]
        public Sprite Icon;
    }
    
    /// <summary>
    /// Simplified representation of a resource cost or amount
    /// </summary>
    [Serializable]
    public class ResourceAmount
    {
        public string ResourceID;
        public float Amount;
    }
} 