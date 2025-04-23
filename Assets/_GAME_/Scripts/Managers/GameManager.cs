using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using Game.Interfaces;

/// <summary>
/// Main controller for the idle game. Handles game state and tick management.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    [Header("Game Configuration")]
    [SerializeField] private float tickInterval = 0.2f; // Time between ticks in seconds

    private float tickTimer;

    [SerializeField] private IResourceManager resourceManager;
    [SerializeField] private IBuildingManager buildingManager;
    [SerializeField] private IUpgradeManager upgradeManager;

    public IResourceManager Resources => resourceManager;
    public IBuildingManager Buildings => buildingManager;
    public IUpgradeManager Upgrades => upgradeManager;

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
    }
    
    private void Start()
    {
        // Initialize any necessary components here
    }

    private void Update()
    {
        // Update the tick timer
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            // Reset the timer
            tickTimer = 0f;

            // Call the tick function
            OnTick();
        }
    }

    /// <summary>
    /// Handler for tick events - main game loop
    /// </summary>
    private void OnTick()
    {
        // Logic to execute on each tick
        Debug.Log("Tick event occurred!");
        // Update game state, resources, etc. here
    }

    public void ResetGame()
    {
        // Logic to reset the game state if needed
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