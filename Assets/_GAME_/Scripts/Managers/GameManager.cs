using System;
using System.Collections.Generic;
using UnityEngine;

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
    // [SerializeField] private TooltipManager tooltipManager;
    // [SerializeField] private GameServices gameServices;
    
    // Properties to access managers
    public ResourceManager Resources => resourceManager;
    public BuildingManager Buildings => buildingManager;
    public UpgradeManager Upgrades => upgradeManager;
    public UIManager UI => uiManager;
    
    // private ResourceCalculator resourceCalculator;
    private DateTime lastSaveTime;
    private float saveTimer;
    
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
        // if (tooltipManager == null) tooltipManager = GetComponent<TooltipManager>();
        // if (gameServices == null) gameServices = GetComponent<GameServices>();
        
        // Register services
        /* if (gameServices != null)
        {
            gameServices.RegisterService<IResourceManager>(resourceManager);
            gameServices.RegisterService<IBuildingManager>(buildingManager);
            gameServices.RegisterService<IUpgradeManager>(upgradeManager);
            gameServices.RegisterService<IUIManager>(uiManager);
        } */
        
        // Create resource calculator - will need to be updated once ResourceCalculator is implemented
        // resourceCalculator = new ResourceCalculator(resourceManager, buildingManager, upgradeManager);
    }
    
    private void Start()
    {
        LoadGame();
        
        // Initialize all managers
        Resources.Initialize();
        Buildings.Initialize();
        Upgrades.Initialize();
        UI.Initialize();
        
        // Process offline progress if needed
        // ProcessOfflineProgress();
    }
    
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // Core game loop - update all systems
        Resources.Tick(deltaTime);
        Buildings.Tick(deltaTime);
        Upgrades.Tick(deltaTime);
        
        // Auto-save logic
        saveTimer += deltaTime;
        if (saveTimer >= saveInterval)
        {
            SaveGame();
            saveTimer = 0f;
        }
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
        
        // Process resource gains for visible resources
        Dictionary<string, float> offlineGains = new Dictionary<string, float>();
        foreach (var resource in Resources.GetVisibleResources())
        {
            /* float gain = resourceCalculator.CalculateOfflineProgress(resource.Definition.ID, offlineSeconds);
            if (gain > 0)
            {
                Resources.AddResource(resource.Definition.ID, gain);
                offlineGains[resource.Definition.ID] = gain;
            } */
        }
        
        // Show offline progress summary to player
        // TODO: Create UI for showing offline progress
        Debug.Log($"Processed offline progress for {offlineSeconds:F0} seconds");
        foreach (var gain in offlineGains)
        {
            Debug.Log($"Gained {gain.Value:F0} {gain.Key}");
        }
    }
    
    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame()
    {
        GameData data = new GameData();
        
        // Collect data from all managers
        data.resourceData = Resources.SerializeData();
        data.buildingData = Buildings.SerializeData();
        data.upgradeData = Upgrades.SerializeData();
        
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
    /// Generate a checksum to validate save data
    /// </summary>
    private string GenerateChecksum(GameData data)
    {
        // Create a simple hash of key game state elements
        string stateString = $"{data.lastSaveTime}{data.resourceData.GetHashCode()}{data.buildingData.GetHashCode()}{data.upgradeData.GetHashCode()}";
        
        // Use a simple hash function for now
        // In a real game, you'd use a more robust cryptographic hash
        int hash = 0;
        foreach (char c in stateString)
        {
            hash = (hash * 31) + c;
        }
        
        return hash.ToString("X8");
    }
    
    /// <summary>
    /// Load a saved game
    /// </summary>
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            GameData data = JsonUtility.FromJson<GameData>(json);
            
            // Validate checksum to prevent tampering
            string expectedChecksum = GenerateChecksum(data);
            if (data.checksum != expectedChecksum)
            {
                Debug.LogWarning("Save data validation failed. Possible corruption or tampering.");
                // Continue loading anyway, but flag the issue
            }
            
            // Parse saved time
            if (!string.IsNullOrEmpty(data.lastSaveTime))
            {
                DateTime.TryParse(data.lastSaveTime, out lastSaveTime);
            }
            
            // Distribute data to managers
            Resources.DeserializeData(data.resourceData);
            Buildings.DeserializeData(data.buildingData);
            Upgrades.DeserializeData(data.upgradeData);
            
            Debug.Log("Game loaded!");
        }
        else
        {
            Debug.Log("No save data found, starting new game.");
            lastSaveTime = DateTime.Now; // Set current time to prevent offline progress on first launch
        }
    }
    
    /// <summary>
    /// Reset the game to initial state
    /// </summary>
    public void ResetGame()
    {
        PlayerPrefs.DeleteKey("SaveData");
        PlayerPrefs.Save();
        
        // Reset all managers to initial state
        Resources.Reset();
        Buildings.Reset();
        Upgrades.Reset();
        
        lastSaveTime = DateTime.Now; // Reset last save time
        
        // Update UI
        UI.RefreshResourceView();
        UI.RefreshBuildingView();
        UI.RefreshUpgradeView();
        
        Debug.Log("Game reset!");
    }
}

/// <summary>
/// Container for all game save data.
/// </summary>
[Serializable]
public class GameData
{
    public ResourceData resourceData;
    public BuildingData buildingData;
    public UpgradeData upgradeData;
    public string lastSaveTime;
    public string checksum;
}