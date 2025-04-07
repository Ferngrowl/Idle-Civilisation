using System;
using UnityEngine;
using GameConfiguration;

namespace Game.Models
{
    /// <summary>
    /// Runtime instance of a resource
    /// </summary>
    [Serializable]
    public class Resource
    {
        public GameConfiguration.ResourceDefinition Definition { get; private set; }
        public float Amount { get; set; }
        public float Capacity { get; set; }
        public bool IsUnlocked { get; set; }
        
        // Delegate for custom visibility conditions
        public Func<bool> VisibilityCondition { get; set; } = () => true;
        
        public bool IsVisible => IsUnlocked && (Definition.VisibleByDefault || VisibilityCondition());
        
        public Resource(GameConfiguration.ResourceDefinition definition)
        {
            Definition = definition;
            Amount = definition.InitialAmount;
            Capacity = definition.InitialCapacity;
            IsUnlocked = definition.VisibleByDefault;
        }
        
        public void Reset()
        {
            Amount = Definition.InitialAmount;
            Capacity = Definition.InitialCapacity;
            IsUnlocked = Definition.VisibleByDefault;
        }
    }
} 