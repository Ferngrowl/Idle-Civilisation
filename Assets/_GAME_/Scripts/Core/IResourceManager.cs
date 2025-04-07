using System.Collections.Generic;
using Game.Models;
using Serialization;
using GameConfiguration;

namespace Game.Interfaces
{
    /// <summary>
    /// Interface for the ResourceManager component
    /// Manages all in-game resources, their production, consumption and capacities
    /// </summary>
    public interface IResourceManager
    {
        // Resource access
        float GetAmount(string resourceID);
        float GetCapacity(string resourceID);
        float GetProductionRate(string resourceID);
        float GetConsumptionRate(string resourceID);
        
        // Resource modification
        void AddResource(string resourceID, float amount);
        void SetCapacity(string resourceID, float capacity);
        
        // Cost handling
        bool CanAfford(Dictionary<string, float> costs);
        void SpendResources(Dictionary<string, float> costs);
        
        // Resource visibility and definitions
        List<Game.Models.Resource> GetVisibleResources();
        GameConfiguration.ResourceDefinition GetResourceDefinition(string id);
        
        // Lifecycle methods
        void Initialize();
        void Tick(long tickNumber);
        
        // Save/load
        ResourceSaveData SerializeData();
        void DeserializeData(ResourceSaveData data);
    }
} 