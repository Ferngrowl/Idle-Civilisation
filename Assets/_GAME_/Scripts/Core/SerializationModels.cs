using System;
using System.Collections.Generic;

/// <summary>
/// Contains data models for game serialization
/// </summary>
namespace Serialization
{
    /// <summary>
    /// Serializable data for resources
    /// </summary>
    [Serializable]
    public class ResourceSaveData
    {
        public List<ResourceData> Resources = new List<ResourceData>();
        
        [Serializable]
        public class ResourceData
        {
            public string ID;
            public float Amount;
            public float Capacity;
            public bool IsUnlocked;
        }
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
    
    /// <summary>
    /// Serializable data for time system
    /// </summary>
    [Serializable]
    public class TimeSaveData
    {
        public long TotalTicks;
        public int Day;
        public int Season;
        public int Weather;
        public int Year;
        public DateTime LastSaveTime;
    }
    
    /// <summary>
    /// Master save data container for the entire game
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Player metadata
        public string PlayerName = "Player";
        public DateTime LastSaveTime;
        public string Checksum;
        
        // Game system data
        public ResourceSaveData Resources = new ResourceSaveData();
        public BuildingSaveData Buildings = new BuildingSaveData();
        public UpgradeSaveData Upgrades = new UpgradeSaveData();
        public TimeSaveData Time = new TimeSaveData();
    }
} 