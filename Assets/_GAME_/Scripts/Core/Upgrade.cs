using System;
using GameConfiguration;

namespace Game.Models
{
    /// <summary>
    /// Represents a runtime instance of an upgrade
    /// </summary>
    [Serializable]
    public class Upgrade
    {
        public UpgradeDefinition Definition { get; private set; }
        public bool IsPurchased { get; set; }
        public bool IsUnlocked { get; set; }
        
        public Func<bool> VisibilityCondition { get; set; } = () => true;
        
        public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition());
        
        public Upgrade(UpgradeDefinition definition)
        {
            Definition = definition;
            IsPurchased = false;
            IsUnlocked = definition.VisibleByDefault;
        }
        
        public void Reset()
        {
            IsPurchased = false;
            IsUnlocked = Definition.VisibleByDefault;
        }
        
        /// <summary>
        /// Get the cost to purchase this upgrade
        /// </summary>
        public Dictionary<string, float> GetCost()
        {
            Dictionary<string, float> cost = new Dictionary<string, float>();
            
            foreach (var resourceCost in Definition.Costs)
            {
                cost[resourceCost.ResourceID] = resourceCost.Amount;
            }
            
            return cost;
        }
    }
} 