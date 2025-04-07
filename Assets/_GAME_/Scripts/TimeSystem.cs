using UnityEngine;

/// <summary>
/// Static utility class for time system operations
/// </summary>
public static class TimeSystem
{
    // Constants for time conversion
    public const int TICKS_PER_SECOND = 5;
    public const int TICKS_PER_DAY = 10;
    public const int DAYS_PER_SEASON = 100;
    public const int SEASONS_PER_YEAR = 4;
    
    // Names for display
    public static readonly string[] SeasonNames = { "Spring", "Summer", "Autumn", "Winter" };
    public static readonly string[] WeatherNames = { "Warm", "Average", "Cold" };
    
    /// <summary>
    /// Convert real seconds to game ticks
    /// </summary>
    public static int SecondsToTicks(float seconds)
    {
        return Mathf.FloorToInt(seconds * TICKS_PER_SECOND);
    }
    
    /// <summary>
    /// Convert game ticks to real seconds
    /// </summary>
    public static float TicksToSeconds(int ticks)
    {
        return ticks / (float)TICKS_PER_SECOND;
    }
    
    /// <summary>
    /// Convert ticks to days
    /// </summary>
    public static int TicksToDays(long ticks)
    {
        return (int)(ticks / TICKS_PER_DAY);
    }
    
    /// <summary>
    /// Calculate season from total days
    /// </summary>
    public static int DaysToSeason(int days)
    {
        return (days / DAYS_PER_SEASON) % SEASONS_PER_YEAR;
    }
    
    /// <summary>
    /// Calculate year from total days
    /// </summary>
    public static int DaysToYears(int days)
    {
        return days / (DAYS_PER_SEASON * SEASONS_PER_YEAR);
    }
    
    /// <summary>
    /// Calculate day of season from total days
    /// </summary>
    public static int DayOfSeason(int days)
    {
        return days % DAYS_PER_SEASON;
    }
} 