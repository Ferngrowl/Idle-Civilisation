using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConfiguration
{
    /// <summary>
    /// Represents resource production or consumption for a building
    /// </summary>
    [Serializable]
    public class ResourceEffect
    {
        public string ResourceID;
        public float Amount;
    }

    /// <summary>
    /// Represents a cost for building construction
    /// </summary>
    [Serializable]
    public class BuildingCost
    {
        public string ResourceID;
        public float BaseAmount;
        public float ScalingFactor = 1.15f;
    }

    /// <summary>
    /// ScriptableObject containing building configuration data
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Building Identity")]
        [Tooltip("Unique identifier for the building")]
        public string ID;
        
        [Tooltip("Display name shown in UI")]
        public string DisplayName;
        
        [Tooltip("Description of the building")]
        [TextArea(2, 5)]
        public string Description;
        
        [Header("Building Properties")]
        [Tooltip("Resources this building produces per second")]
        public List<ResourceEffect> Production = new List<ResourceEffect>();
        
        [Tooltip("Resources this building consumes per second")]
        public List<ResourceEffect> Consumption = new List<ResourceEffect>();
        
        [Tooltip("Resources required to build this building")]
        public List<BuildingCost> Costs = new List<BuildingCost>();
        
        [Header("Visibility & Requirements")]
        [Tooltip("Whether this building is visible from the start")]
        public bool VisibleByDefault = false;
        
        [Tooltip("Resources required to unlock this building")]
        public List<string> RequiredResources = new List<string>();
        
        [Tooltip("Other buildings required to unlock this building")]
        public List<string> RequiredBuildings = new List<string>();
        
        [Tooltip("Upgrades required to unlock this building")]
        public List<string> RequiredUpgrades = new List<string>();
        
        [Header("UI Display")]
        [Tooltip("Icon for the building")]
        public Sprite Icon;
        
        [Tooltip("Color for the building in UI")]
        public Color Color = Color.white;

        [Header("Storage")]
        [Tooltip("Storage capacity increases from this building")]
        public List<ResourceEffect> Capacity = new List<ResourceEffect>();
    }
} 