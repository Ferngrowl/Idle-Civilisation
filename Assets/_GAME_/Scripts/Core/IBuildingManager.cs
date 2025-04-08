using System.Collections.Generic;
using Game.Models;
using GameConfiguration;
using Serialization;

namespace Game.Interfaces
{
    /// <summary>
    /// Interface for the BuildingManager component
    /// Manages buildings, their construction, and effects
    /// </summary>
    public interface IBuildingManager
    {
        // Building access - use fully qualified type
        Game.Models.Building GetBuilding(string buildingID);
        List<Game.Models.Building> GetAllBuildings();
        List<Game.Models.Building> GetVisibleBuildings();
        int GetBuildingCount(string buildingID);
        GameConfiguration.BuildingDefinition GetBuildingDefinition(string buildingID);
        
        // Building operations
        bool CanConstructBuilding(string buildingID);
        bool ConstructBuilding(string buildingID);
        Dictionary<string, float> CalculateBuildingCost(string buildingID);
        
        // Lifecycle methods
        void Initialize();
        void Tick(long tickNumber);
        void Reset();
        
        // Save/load
        BuildingSaveData SerializeData();
        void DeserializeData(BuildingSaveData data);
    }
} 