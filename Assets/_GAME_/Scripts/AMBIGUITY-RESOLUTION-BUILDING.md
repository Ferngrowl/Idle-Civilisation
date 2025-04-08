# BuildingDefinition Ambiguity Resolution Plan

## Current Situation

We have inconsistency between BuildingDefinition and BuildingData with several issues:

1. In GameConfiguration namespace (BuildingData.cs):
   ```csharp
   [CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building Definition")]
   public class BuildingDefinition : ScriptableObject
   {
       public string ID;
       public string DisplayName;
       // Other properties for the building definition
   }
   ```

2. In Game.Models namespace (Building.cs):
   ```csharp
   public class Building
   {
       public BuildingData Definition { get; private set; }
       // But Building is initialized with BuildingDefinition in BuildingManager!
   }
   ```

3. In BuildingManager.cs:
   ```csharp
   [SerializeField] private List<BuildingDefinition> buildingDefinitions = new List<BuildingDefinition>();
   // Using BuildingDefinition here, but passing to Building class which expects BuildingData
   
   private Dictionary<string, GameConfiguration.BuildingDefinition> cachedBuildingDefinitions = 
       new Dictionary<string, GameConfiguration.BuildingDefinition>();
   // Using GameConfiguration.BuildingDefinition here, suggesting a conversion is needed
   ```

This creates a confusing situation where:
- The canonical class name in GameConfiguration is BuildingDefinition
- Building.cs expects a BuildingData property
- BuildingManager.cs has workarounds for conversion between types

## Implementation Plan

### Step 1: Update Game.Models.Building to Use Correct Type

```csharp
namespace Game.Models
{
    public class Building
    {
        // Change from BuildingData to GameConfiguration.BuildingDefinition
        public GameConfiguration.BuildingDefinition Definition { get; private set; }
        
        // Update constructor
        public Building(GameConfiguration.BuildingDefinition definition)
        {
            Definition = definition;
            Count = 0;
            IsUnlocked = definition.VisibleByDefault;
        }
        
        // Update other methods as needed
    }
}
```

### Step 2: Update BuildingManager to Use Consistent Types

```csharp
public class BuildingManager : MonoBehaviour, IBuildingManager
{
    // Use fully qualified type
    [SerializeField] private List<GameConfiguration.BuildingDefinition> buildingDefinitions = 
        new List<GameConfiguration.BuildingDefinition>();
    
    // Use fully qualified type
    private Dictionary<string, Game.Models.Building> buildings = 
        new Dictionary<string, Game.Models.Building>();
    
    // Remove the cachedBuildingDefinitions dictionary and GetConfigBuildingDefinition method
    // since they're now redundant
    
    public void Initialize()
    {
        // Initialize dependencies
        
        // Create Building instances directly from the definitions
        foreach (var definition in buildingDefinitions)
        {
            buildings[definition.ID] = new Game.Models.Building(definition);
        }
        
        // Set up visibility conditions
    }
    
    // Update GetBuildingDefinition method
    public GameConfiguration.BuildingDefinition GetBuildingDefinition(string buildingID)
    {
        if (buildings.ContainsKey(buildingID))
        {
            return buildings[buildingID].Definition;
        }
        return null;
    }
}
```

### Step 3: Update IBuildingManager Interface (if needed)

Ensure the interface uses the fully qualified type:

```csharp
public interface IBuildingManager
{
    // Building access
    Building GetBuilding(string buildingID);
    List<Building> GetAllBuildings();
    List<Building> GetVisibleBuildings();
    int GetBuildingCount(string buildingID);
    GameConfiguration.BuildingDefinition GetBuildingDefinition(string buildingID);
    
    // Other methods...
}
```

### Step 4: Remove Redundant Types and Methods

1. Remove any BuildingData class if it exists (or rename it to BuildingDefinition)
2. Remove any redundant conversion methods
3. Remove any caching mechanisms that were used as workarounds

### Step 5: Testing

1. Compile the code and fix any remaining errors
2. Test building-related functionality:
   - Building construction
   - Resource production from buildings
   - Building costs and scaling
   - Building UI display
   - Save/load building data

## Implementation Notes

- This approach is similar to our ResourceDefinition resolution
- Focus on making Game.Models.Building correctly reference GameConfiguration.BuildingDefinition
- Remove any hacky workarounds like the type conversion cache in BuildingManager
- Test thoroughly as building functionality is core to the game

## Expected Outcome

- All building-related code will use consistent type names
- Game.Models.Building will properly reference GameConfiguration.BuildingDefinition
- No need for conversion methods or caching
- Cleaner code with fewer ambiguity errors 