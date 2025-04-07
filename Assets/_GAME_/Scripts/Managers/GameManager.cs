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
    public ITimeManager Time => timeManager;
    
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
        if (timeManager == null) timeManager = GetComponent<TimeManager>();
        
        // Register services with ServiceLocator
        ServiceLocator.Register<IResourceManager>((IResourceManager)resourceManager);
        ServiceLocator.Register<IBuildingManager>((IBuildingManager)buildingManager);
        ServiceLocator.Register<IUpgradeManager>((IUpgradeManager)upgradeManager);
        ServiceLocator.Register<IUIManager>((IUIManager)uiManager);
        ServiceLocator.Register<ITimeManager>((ITimeManager)timeManager);
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
    private void SetupTimeEvents()
    {
        timeManager.OnTick += OnTick;
        timeManager.OnNewDay += OnNewDay;
        timeManager.OnSeasonChange += OnSeasonChange;
        timeManager.OnWeatherChange += OnWeatherChange;
        timeManager.OnNewYear += OnNewYear;
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
    
    /// <summary>
    /// Handler for day change events
    /// </summary>
    private void OnNewDay()
    {
        // Daily events like random encounters could happen here
        Debug.Log($"New day: {timeManager.GetDateString()}");
    }
    
    /// <summary>
    /// Handler for season change events
    /// </summary>
    private void OnSeasonChange(int season)
    {
        // Update UI and trigger any season-specific events
        Debug.Log($"Season changed to {season}");
    }
    
    /// <summary>
    /// Handler for weather change events
    /// </summary>
    private void OnWeatherChange(int weather)
    {
        // Update UI and trigger any weather-specific events
        Debug.Log($"Weather changed to {weather}");
    }
    
    /// <summary>
    /// Handler for new year events
    /// </summary>
    private void OnNewYear()
    {
        // New year events
        Debug.Log($"New year: {timeManager.Year}");
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
}

/// <summary>
/// UI tab types
/// </summary>
public enum UITab
{
    Buildings,
    Upgrades
}