using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI display for the time system (days, seasons, years, weather)
/// </summary>
public class TimeUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text seasonText;
    [SerializeField] private TMP_Text weatherText;
    [SerializeField] private Image seasonIcon;
    [SerializeField] private Image weatherIcon;
    
    [Header("Icons")]
    [SerializeField] private Sprite springSprite;
    [SerializeField] private Sprite summerSprite;
    [SerializeField] private Sprite autumnSprite;
    [SerializeField] private Sprite winterSprite;
    [SerializeField] private Sprite warmSprite;
    [SerializeField] private Sprite averageSprite;
    [SerializeField] private Sprite coldSprite;
    
    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Time != null)
        {
            // Subscribe to time events
            var timeManager = GameManager.Instance.Time;
            timeManager.OnNewDay += UpdateDateDisplay;
            timeManager.OnSeasonChange += UpdateSeason;
            timeManager.OnWeatherChange += UpdateWeather;
            
            // Update with initial values
            UpdateDateDisplay();
            UpdateSeason(timeManager.CurrentSeason);
            UpdateWeather(timeManager.CurrentWeather);
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Time != null)
        {
            // Unsubscribe from time events
            var timeManager = GameManager.Instance.Time;
            timeManager.OnNewDay -= UpdateDateDisplay;
            timeManager.OnSeasonChange -= UpdateSeason;
            timeManager.OnWeatherChange -= UpdateWeather;
        }
    }
    
    /// <summary>
    /// Update the date display
    /// </summary>
    private void UpdateDateDisplay()
    {
        if (GameManager.Instance == null || dateText == null) return;
        
        var timeManager = GameManager.Instance.Time;
        if (timeManager == null) return;
        
        // Update date text
        dateText.text = timeManager.GetDateString();
    }
    
    /// <summary>
    /// Update the season display
    /// </summary>
    private void UpdateSeason(int season)
    {
        if (seasonText == null) return;
        
        // Update season text
        seasonText.text = GetSeasonName(season);
        seasonText.color = GetSeasonColor(season);
        
        // Update season icon
        if (seasonIcon != null)
        {
            seasonIcon.sprite = GetSeasonSprite(season);
        }
    }
    
    /// <summary>
    /// Update the weather display
    /// </summary>
    private void UpdateWeather(int weather)
    {
        if (weatherText == null) return;
        
        // Get time manager
        var timeManager = GameManager.Instance?.Time;
        if (timeManager == null) return;
        
        // Only show weather after year 4
        if (timeManager.Year < 4)
        {
            weatherText.text = "Average";
            if (weatherIcon != null)
            {
                weatherIcon.sprite = averageSprite;
            }
            weatherText.color = GetWeatherColor(1); // 1 = Average
        }
        else
        {
            weatherText.text = GetWeatherName(weather);
            weatherText.color = GetWeatherColor(weather);
            
            // Update weather icon
            if (weatherIcon != null)
            {
                weatherIcon.sprite = GetWeatherSprite(weather);
            }
        }
    }
    
    /// <summary>
    /// Get season name from integer
    /// </summary>
    private string GetSeasonName(int season)
    {
        if (season >= 0 && season < TimeSystem.SeasonNames.Length)
            return TimeSystem.SeasonNames[season];
        return "Unknown";
    }
    
    /// <summary>
    /// Get weather name from integer
    /// </summary>
    private string GetWeatherName(int weather)
    {
        if (weather >= 0 && weather < TimeSystem.WeatherNames.Length)
            return TimeSystem.WeatherNames[weather];
        return "Unknown";
    }
    
    /// <summary>
    /// Get season sprite from integer
    /// </summary>
    private Sprite GetSeasonSprite(int season)
    {
        switch (season)
        {
            case 0: return springSprite;
            case 1: return summerSprite;
            case 2: return autumnSprite;
            case 3: return winterSprite;
            default: return null;
        }
    }
    
    /// <summary>
    /// Get weather sprite from integer
    /// </summary>
    private Sprite GetWeatherSprite(int weather)
    {
        switch (weather)
        {
            case 0: return warmSprite;
            case 1: return averageSprite;
            case 2: return coldSprite;
            default: return null;
        }
    }
    
    /// <summary>
    /// Get color for season display
    /// </summary>
    private Color GetSeasonColor(int season)
    {
        switch (season)
        {
            case 0: // Spring
                return new Color(0.4f, 0.8f, 0.4f); // Green
            case 1: // Summer
                return new Color(1f, 0.8f, 0.2f); // Yellow
            case 2: // Autumn
                return new Color(0.9f, 0.5f, 0.2f); // Orange
            case 3: // Winter
                return new Color(0.6f, 0.8f, 1f); // Light Blue
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get color for weather display
    /// </summary>
    private Color GetWeatherColor(int weather)
    {
        switch (weather)
        {
            case 0: // Warm
                return new Color(1f, 0.6f, 0.2f); // Orange
            case 1: // Average
                return Color.white;
            case 2: // Cold
                return new Color(0.6f, 0.8f, 1f); // Light Blue
            default:
                return Color.white;
        }
    }
} 