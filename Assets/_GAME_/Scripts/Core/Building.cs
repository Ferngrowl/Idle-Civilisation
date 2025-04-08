using System;
using System.Collections.Generic;
using UnityEngine;
using GameConfiguration;

namespace Game.Models
{
    /// <summary>
    /// Represents a runtime instance of a building
    /// </summary>
    [Serializable]
    public class Building
    {
        public GameConfiguration.BuildingDefinition Definition { get; private set; }
        public int Count { get; set; }
        public bool IsUnlocked { get; set; }
        
        // Delegate for custom visibility conditions
        public Func<bool> VisibilityCondition { get; set; } = () => true;
        
        // Property to check if the building should be visible in the UI
        public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition());
        
        public Building(GameConfiguration.BuildingDefinition definition)
        {
            Definition = definition;
            Count = 0;
            IsUnlocked = definition.VisibleByDefault;
        }
        
        public void Reset()
        {
            Count = 0;
            IsUnlocked = Definition.VisibleByDefault;
        }
        
        /// <summary>
        /// Calculate the cost of the next building
        /// </summary>
        public Dictionary<string, float> GetCurrentCost()
        {
            Dictionary<string, float> cost = new Dictionary<string, float>();
            
            foreach (var baseCost in Definition.Costs)
            {
                // Apply price scaling formula: base * (scaling^count)
                cost[baseCost.ResourceID] = baseCost.BaseAmount * 
                    Mathf.Pow(baseCost.ScalingFactor, Count);
            }
            
            return cost;
        }
    }
} 