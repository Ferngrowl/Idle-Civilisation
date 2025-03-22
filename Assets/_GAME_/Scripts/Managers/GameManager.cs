using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for the idle game. Handles game state, saving/loading, and time management.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private float saveInterval = 60f;
    private float saveTimer;
    
    public ResourceManager Resources { get; private set; }
    public BuildingManager Buildings { get; private set; }
    public UpgradeManager Upgrades { get; private set; }
    public UIManager UI { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize managers
        Resources = GetComponent<ResourceManager>();
        Buildings = GetComponent<BuildingManager>();
        Upgrades = GetComponent<UpgradeManager>();
        UI = GetComponent<UIManager>();
    }
    
    private void Start()
    {
        LoadGame();
        
        // Initialize all managers
        Resources.Initialize();
        Buildings.Initialize();
        Upgrades.Initialize();
        UI.Initialize();
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
    
    public void SaveGame()
    {
        GameData data = new GameData();
        
        // Collect data from all managers
        data.resourceData = Resources.SerializeData();
        data.buildingData = Buildings.SerializeData();
        data.upgradeData = Upgrades.SerializeData();
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
        
        Debug.Log("Game saved!");
    }
    
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            GameData data = JsonUtility.FromJson<GameData>(json);
            
            // Distribute data to managers
            Resources.DeserializeData(data.resourceData);
            Buildings.DeserializeData(data.buildingData);
            Upgrades.DeserializeData(data.upgradeData);
            
            Debug.Log("Game loaded!");
        }
        else
        {
            Debug.Log("No save data found, starting new game.");
        }
    }
    
    public void ResetGame()
    {
        PlayerPrefs.DeleteKey("SaveData");
        PlayerPrefs.Save();
        
        // Reset all managers to initial state
        Resources.Reset();
        Buildings.Reset();
        Upgrades.Reset();
        
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
}