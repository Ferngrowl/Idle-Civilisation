using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;
using Serialization;

/// <summary>
/// Manages the time system including ticks, days, seasons, years, and weather.
/// </summary>
public class TimeManager : MonoBehaviour, ITimeManager
{
    private static TimeManager _instance;
    public static TimeManager Instance => _instance;
    
    [Header("Time Configuration")]
    [SerializeField] private float tickDuration = 0.2f; // 5 ticks per second
    
    // Current time state
    private float tickTimer = 0f;
    private long totalTicks = 0;
    private int day = 0;
    private int currentSeason = 0; // 0=Spring, 1=Summer, 2=Autumn, 3=Winter
    private int year = 0;
    private int currentWeather = 1; // 0=Warm, 1=Average, 2=Cold
    
    // Events
    public event Action OnTick;
    public event Action OnNewDay;
    public event Action<int> OnSeasonChange;
    public event Action<int> OnWeatherChange;
    public event Action OnNewYear;
    
    // Season modifiers for resource production
    // Each array maps to [Spring, Summer, Autumn, Winter]
    // Each inner array maps to [Warm, Average, Cold]
    private readonly float[,] seasonalCatnipModifiers = new float[,] {
        { 0.65f, 0.5f, 0.35f },     // Spring
        { 0.15f, 0f, -0.15f },      // Summer
        { 0.15f, 0f, -0.15f },      // Autumn
        { -0.6f, -0.75f, -0.9f }    // Winter
    };
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
    }
    
    private void Start()
    {
        // Initialize with default season and weather
        currentSeason = 0; // Spring
        currentWeather = 1; // Average
        
        // Register with ServiceLocator
        ServiceLocator.Register<ITimeManager>(this);
    }
    
    private void Update()
    {
        // Progress time based on real time
        tickTimer += UnityEngine.Time.deltaTime;
        
        // Execute tick when timer exceeds duration
        if (tickTimer >= tickDuration)
        {
            ExecuteTick();
            tickTimer -= tickDuration;
        }
    }
    
    /// <summary>
    /// Execute a single tick of game time
    /// </summary>
    private void ExecuteTick()
    {
        totalTicks++;
        
        // Notify listeners of tick
        OnTick?.Invoke();
        
        // Check for day change (10 ticks = 1 day)
        if (totalTicks % TimeSystem.TICKS_PER_DAY == 0)
        {
            day++;
            OnNewDay?.Invoke();
            
            // Check for season change (100 days = 1 season)
            if (day % TimeSystem.DAYS_PER_SEASON == 0)
            {
                NextSeason();
                
                // Check for year change (4 seasons = 1 year)
                if (currentSeason == 0 && day > 0) // Spring = 0
                {
                    year++;
                    OnNewYear?.Invoke();
                    
                    // After year 4, weather can change
                    if (year >= 4)
                    {
                        DetermineWeather();
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Advance to next season in the cycle
    /// </summary>
    private void NextSeason()
    {
        int oldSeason = currentSeason;
        
        // Update season (Spring -> Summer -> Autumn -> Winter -> Spring)
        currentSeason = (currentSeason + 1) % TimeSystem.SEASONS_PER_YEAR;
        
        // Notify listeners of season change
        if (oldSeason != currentSeason)
        {
            DetermineWeather(); // Change weather when season changes
            OnSeasonChange?.Invoke(currentSeason);
        }
    }
    
    /// <summary>
    /// Determine the weather condition for the current season
    /// </summary>
    private void DetermineWeather()
    {
        int oldWeather = currentWeather;
        
        // 65% chance of average weather, 17.5% chance each of warm or cold
        float roll = UnityEngine.Random.Range(0f, 1f);
        
        // Weather probability (35% chance of abnormal weather)
        const float ABNORMAL_WEATHER_CHANCE = 0.35f;
        
        if (roll < ABNORMAL_WEATHER_CHANCE / 2)
        {
            currentWeather = 0; // Warm
        }
        else if (roll < ABNORMAL_WEATHER_CHANCE)
        {
            currentWeather = 2; // Cold
        }
        else
        {
            currentWeather = 1; // Average
        }
        
        // Notify listeners of weather change
        if (oldWeather != currentWeather)
        {
            OnWeatherChange?.Invoke(currentWeather);
        }
    }
    
    /// <summary>
    /// Get the current season resource production modifier
    /// </summary>
    /// <param name="resourceID">The resource to get modifier for</param>
    /// <returns>Seasonal resource multiplier</returns>
    public float GetSeasonalModifier(string resourceID)
    {
        // Currently only catnip is affected by seasons
        if (resourceID == "catnip")
        {
            return 1f + seasonalCatnipModifiers[currentSeason, currentWeather];
        }
        
        return 1f;
    }
    
    /// <summary>
    /// Get formatted string displaying current date
    /// </summary>
    public string GetDateString()
    {
        return $"Year {year}, {TimeSystem.SeasonNames[currentSeason]}, Day {day % TimeSystem.DAYS_PER_SEASON}";
    }
    
    /// <summary>
    /// Get formatted string displaying current weather
    /// </summary>
    public string GetWeatherString()
    {
        if (year < 4) return "Average";
        return TimeSystem.WeatherNames[currentWeather];
    }
    
    /// <summary>
    /// Advance time by one tick (for offline progress calculation)
    /// </summary>
    public void AdvanceTime()
    {
        ExecuteTick();
    }
    
    /// <summary>
    /// Get time data for save system
    /// </summary>
    public TimeSaveData SerializeData()
    {
        return new TimeSaveData
        {
            TotalTicks = totalTicks,
            Day = day,
            Year = year,
            Season = currentSeason,
            Weather = currentWeather,
            LastSaveTime = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Load time data from save system
    /// </summary>
    public void DeserializeData(TimeSaveData data)
    {
        if (data == null) return;
        
        totalTicks = data.TotalTicks;
        day = data.Day;
        year = data.Year;
        currentSeason = data.Season;
        currentWeather = data.Weather;
    }
    
    /// <summary>
    /// Reset time to initial state
    /// </summary>
    public void Reset()
    {
        totalTicks = 0;
        day = 0;
        year = 0;
        currentSeason = 0;
        currentWeather = 1;
        tickTimer = 0f;
    }
    
    // GETTERS
    public long TotalTicks => totalTicks;
    public int Day => day;
    public int DayOfSeason => day % TimeSystem.DAYS_PER_SEASON;
    public int CurrentSeason => currentSeason;
    public int CurrentWeather => currentWeather;
    public int Year => year;
}

/// <summary>
/// Seasons in the game
/// </summary>
public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

/// <summary>
/// Weather conditions affecting resource production
/// </summary>
public enum WeatherCondition
{
    Warm,
    Average,
    Cold
} 