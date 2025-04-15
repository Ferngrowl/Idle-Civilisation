using System;
using UnityEngine;
using Game.Interfaces;
using Serialization;

/// <summary>
/// Container for all game save data
/// </summary>
[Serializable]
public class GameData
{
    public Serialization.ResourceSaveData resourceData;
    public Serialization.BuildingSaveData buildingData;
    public Serialization.UpgradeSaveData upgradeData;
    public Serialization.TimeSaveData timeData;
    public string lastSaveTime;
    public int checksum;
}

/// <summary>
/// Handles saving and loading game data, separate from gameplay managers
/// </summary>
public class SaveSystem : MonoBehaviour
{
    [Header("Save Configuration")]
    [SerializeField] private float saveInterval = 60f;
    [SerializeField] private float maxOfflineTime = 86400f; // 24 hours
    
    private float saveTimer;
    private DateTime lastSaveTime;
    
    // Dependencies accessed via ServiceLocator
    private IResourceManager resourceManager;
    private IBuildingManager buildingManager;
    private IUpgradeManager upgradeManager;
    private ITimeManager timeManager;
    
    private void Start()
    {
        // Get dependencies from ServiceLocator
        resourceManager = ServiceLocator.Get<IResourceManager>();
        buildingManager = ServiceLocator.Get<IBuildingManager>();
        upgradeManager = ServiceLocator.Get<IUpgradeManager>();
        timeManager = ServiceLocator.Get<ITimeManager>();
        
        // Load any existing save
        LoadGame();
    }
    
    private void Update()
    {
        // Auto-save logic
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveGame();
            saveTimer = 0f;
        }
    }
    
    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame()
    {
        if (resourceManager == null || buildingManager == null || 
            upgradeManager == null || timeManager == null)
        {
            Debug.LogError("Cannot save game: one or more managers not found.");
            return;
        }
        
        GameData data = new GameData();
        
        // Collect data from all managers
        data.resourceData = resourceManager.SerializeData();
        data.buildingData = buildingManager.SerializeData();
        data.upgradeData = upgradeManager.SerializeData();
        data.timeData = timeManager.SerializeData();
        
        // Store current time for offline progress calculation
        lastSaveTime = DateTime.Now;
        data.lastSaveTime = lastSaveTime.ToString("o");
        
        // Generate checksum for save data validation
        data.checksum = GenerateChecksum(data);
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
        
        Debug.Log("Game saved!");
    }
    
    /// <summary>
    /// Load a saved game
    /// </summary>
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SaveData"))
        {
            Debug.Log("No save data found.");
            return;
        }
        
        string json = PlayerPrefs.GetString("SaveData");
        GameData data = JsonUtility.FromJson<GameData>(json);
        
        if (data == null)
        {
            Debug.LogError("Failed to parse save data.");
            return;
        }
        
        // Validate checksum
        int calculatedChecksum = GenerateChecksum(data);
        if (calculatedChecksum != data.checksum)
        {
            Debug.LogWarning("Save data checksum mismatch. Save data may be corrupted.");
            // Continue loading anyway for now, but could add user confirmation here
        }
        
        // Parse last save time
        if (!string.IsNullOrEmpty(data.lastSaveTime))
        {
            DateTime.TryParse(data.lastSaveTime, out lastSaveTime);
        }
        
        // Deserialize data to managers
        if (resourceManager != null) resourceManager.DeserializeData(data.resourceData);
        if (buildingManager != null) buildingManager.DeserializeData(data.buildingData);
        if (upgradeManager != null) upgradeManager.DeserializeData(data.upgradeData);
        if (timeManager != null) timeManager.DeserializeData(data.timeData);
        
        Debug.Log("Game loaded!");
        
        // Process offline progress
        ProcessOfflineProgress();
    }
    
    /// <summary>
    /// Process resource gains during offline time
    /// </summary>
    private void ProcessOfflineProgress()
    {
        if (resourceManager == null || lastSaveTime == default(DateTime))
            return;
            
        // Calculate time since last save
        TimeSpan offlineTime = DateTime.Now - lastSaveTime;
        float offlineSeconds = (float)offlineTime.TotalSeconds;
        
        // Skip if offline for less than a second
        if (offlineSeconds < 1f)
            return;
            
        // Cap offline time if needed
        offlineSeconds = Mathf.Min(offlineSeconds, maxOfflineTime);
        
        // Calculate ticks passed
        int offlineTicks = Mathf.FloorToInt(offlineSeconds / 0.2f);
        
        // Simulate the passage of time
        for (int i = 0; i < offlineTicks; i++)
        {
            timeManager.AdvanceTime();
        }
        
        Debug.Log($"Processed offline progress for {offlineSeconds:F0} seconds ({offlineTicks} ticks)");
    }
    
    /// <summary>
    /// Generate a simple checksum to validate save data
    /// </summary>
    private int GenerateChecksum(GameData data)
    {
        // A very simple checksum algorithm
        int checksum = 0;
        
        if (data.lastSaveTime != null) 
        {
            foreach (char c in data.lastSaveTime)
            {
                checksum += (int)c;
            }
        }
        
        return checksum;
    }
} 