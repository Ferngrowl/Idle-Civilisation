using System;
using Serialization;

namespace Game.Interfaces
{
    /// <summary>
    /// Interface for the TimeManager component
    /// Manages game time, seasons, weather and other time-based systems
    /// </summary>
    public interface ITimeManager
    {
        // Properties
        long TotalTicks { get; }
        int Day { get; }
        int DayOfSeason { get; }
        int CurrentSeason { get; }
        int CurrentWeather { get; }
        int Year { get; }
        
        // Events
        event Action OnTick;
        event Action OnNewDay;
        event Action<int> OnSeasonChange;
        event Action<int> OnWeatherChange;
        event Action OnNewYear;
        
        // Methods
        float GetSeasonalModifier(string resourceID);
        string GetDateString();
        string GetWeatherString();
        void AdvanceTime();
        TimeSaveData SerializeData();
        void DeserializeData(TimeSaveData data);
        void Reset();
    }
} 