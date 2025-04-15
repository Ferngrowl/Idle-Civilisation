using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using Game.Interfaces;
using Serialization;

/// <summary>
/// Main controller for the idle game. Handles game state, saving/loading, and time management.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    [Header("Game Configuration")]
    [SerializeField] private float saveInterval = 60f;
    [SerializeField] private float maxOfflineTime = 86400f; // 24 hours
    
    [Header("Manager References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TimeManager timeManager;
    
    private DateTime lastSaveTime;
    private float saveTimer;
    
    // Properties for easy access
    public IResourceManager Resources => resourceManager;
    public IBuildingManager Buildings => buildingManager;
    public IUpgradeManager Upgrades => upgradeManager;
    public IUIManager UI => (IUIManager)uiManager;
    public TimeManager Time => timeManager;
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize components if not set in inspector
        if (resourceManager == null) resourceManager = GetComponent<ResourceManager>();
        if (buildingManager == null) buildingManager = GetComponent<BuildingManager>();
        if (upgradeManager == null) upgradeManager = GetComponent<UpgradeManager>();
        if (uiManager == null) uiManager = GetComponent<UIManager>();
        
        // Register services with ServiceLocator
        ServiceLocator.Register<IResourceManager>(resourceManager);
        ServiceLocator.Register<IBuildingManager>(buildingManager);
        ServiceLocator.Register<IUpgradeManager>(upgradeManager);
        ServiceLocator.Register<IUIManager>(uiManager);
    }
    
    private void Start()
    {
        // Load any existing game data
        LoadGame();
        
        // Initialize all managers
        resourceManager.Initialize();
        buildingManager.Initialize();
        upgradeManager.Initialize();
        uiManager.Initialize();
        
        // Setup Time Manager events
        SetupTimeEvents();
        
        // Process offline progress
        ProcessOfflineProgress();
    }
    
    /// <summary>
    /// Setup event handlers for TimeManager
    /// </summary> 
    private void SetupTimeEvents() // Add this method
    {
        if (timeManager != null)
        {
            timeManager.OnTick += OnTick;
            timeManager.OnNewDay += OnNewDay;
            timeManager.OnSeasonChange += OnSeasonChange;
            timeManager.OnWeatherChange += OnWeatherChange;
            timeManager.OnNewYear += OnNewYear;
        }
    }
   
    
    /// <summary>
    /// Handler for tick events - main game loop
    /// </summary>
    private void OnTick()
    {
        // Update all systems on each tick
        resourceManager.Tick(timeManager.TotalTicks);
        buildingManager.Tick(timeManager.TotalTicks);
        upgradeManager.Tick(timeManager.TotalTicks);
    }
    
    private void Update()
    {
        // Auto-save logic
        saveTimer += UnityEngine.Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveGame();
            saveTimer = 0f;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (timeManager != null)
        {
            timeManager.OnTick -= OnTick;
            timeManager.OnNewDay -= OnNewDay;
            timeManager.OnSeasonChange -= OnSeasonChange;
            timeManager.OnWeatherChange -= OnWeatherChange;
            timeManager.OnNewYear -= OnNewYear;
        }
        
        // Clear service locator
        ServiceLocator.Clear();
    }
    
    /// <summary>
    /// Process resource gains during offline time
    /// </summary>
    private void ProcessOfflineProgress()
    {
        // Skip offline progress if starting a new game
        if (lastSaveTime == default(DateTime))
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
        
        // Advance game time
        for (int i = 0; i < offlineTicks; i++)
        {
            timeManager.AdvanceTime();
        }
        
        // Show offline progress dialog
        // TODO: Implement offline progress UI
    }
    
    /// <summary>
    /// Save game data to persistent storage
    /// </summary>
    public void SaveGame()
    {
        var data = new Serialization.SaveData
        {
            Resources = resourceManager.SerializeData(),
            Buildings = buildingManager.SerializeData(),
            Upgrades = upgradeManager.SerializeData(),
            Time = timeManager.SerializeData(),
            LastSaveTime = DateTime.Now,
            Checksum = ""
        };
        
        // Generate checksum
        data.Checksum = GenerateChecksum(data);
        
        // Convert to JSON
        string json = JsonUtility.ToJson(data);
        
        // Save to PlayerPrefs for now
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
        
        lastSaveTime = DateTime.Now;
        
        Debug.Log("Game saved!");
    }
    
    /// <summary>
    /// Load game data from persistent storage
    /// </summary>
    public void LoadGame()
    {
        // Check if save data exists
        if (!PlayerPrefs.HasKey("SaveData"))
            return;
            
        string json = PlayerPrefs.GetString("SaveData");
        
        // Parse saved data
        var data = JsonUtility.FromJson<Serialization.SaveData>(json);
        
        // Verify checksum
        string originalChecksum = data.Checksum;
        data.Checksum = "";
        string calculatedChecksum = GenerateChecksum(data);
        
        if (originalChecksum != calculatedChecksum)
        {
            Debug.LogWarning("Save data checksum mismatch! Data may be corrupted.");
            return;
        }
        
        // Load data into managers
        resourceManager.DeserializeData(data.Resources);
        buildingManager.DeserializeData(data.Buildings);
        upgradeManager.DeserializeData(data.Upgrades);
        timeManager.DeserializeData(data.Time);
        
        // Set last save time
        lastSaveTime = data.LastSaveTime;
        
        Debug.Log("Game loaded!");
    }
    
    /// <summary>
    /// Generate a checksum for save data verification
    /// </summary>
    private string GenerateChecksum(Serialization.SaveData data)
    {
        // Just do a simple checksum for now
        // Convert the data back to JSON without the checksum
        string dataString = JsonUtility.ToJson(data);
        
        // Compute MD5 hash of the data string
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    
    /// <summary>
    /// Reset the game state to defaults
    /// </summary>
    public void ResetGame()
    {
        // Reset all managers
        resourceManager.Reset();
        buildingManager.Reset();
        upgradeManager.Reset();
        timeManager.Reset();
        
        // Clear save data
        PlayerPrefs.DeleteKey("SaveData");
        
        // Reset last save time
        lastSaveTime = default(DateTime);
        
        // Refresh UI
        uiManager.RefreshResourceView();
        uiManager.RefreshBuildingView();
        uiManager.RefreshUpgradeView();
        
        Debug.Log("Game reset!");
    }

    private void OnNewDay() // Add this method
    {
        // Logic for when a new day starts
        Debug.Log("A new day has started!");
    }

    private void OnSeasonChange(int newSeason) // Add this method
    {
        // Logic for when the season changes
        Debug.Log($"Season changed to: {newSeason}");
    }

    private void OnWeatherChange(int newWeather) // Add this method
    {
        // Logic for when the weather changes
        Debug.Log($"Weather changed to: {newWeather}");
    }

    private void OnNewYear() // Add this method
    {
        // Logic for when a new year starts
        Debug.Log("A new year has started!");
    }
}

/// <summary>
/// UI tab types
/// </summary>
public enum UITab
{
    Buildings,
    Upgrades
}