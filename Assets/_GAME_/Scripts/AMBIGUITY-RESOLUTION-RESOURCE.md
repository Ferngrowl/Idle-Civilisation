# ResourceDefinition Ambiguity Resolution Plan

## Current Situation

We have multiple conflicting definitions of ResourceDefinition:

1. In global namespace (Resource.cs):
   ```csharp
   [Serializable]
   public class ResourceDefinition
   {
       public string ID;
       public string DisplayName;
       public string Description;
       public bool HasCapacity;
       public float InitialAmount;
       public float BaseCapacity;
       public bool VisibleByDefault;
       public string DisplayFormat = "0.0";
       public Sprite Icon;
   }
   ```

2. In ResourceManager.cs (global namespace):
   ```csharp
   [Serializable]
   public class ResourceDefinition
   {
       public string ID;
       public string DisplayName;
       public string Description;
       public Sprite Icon;
       public bool HasCapacity = false;
       public float InitialCapacity = 100f;
       public float InitialAmount = 0f;
       public bool VisibleByDefault = false;
   }
   ```

3. In GameConfiguration namespace (ScriptableObjects/ResourceDefinition.cs):
   ```csharp
   [CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource Definition")]
   public class ResourceDefinition : ScriptableObject
   {
       // Same properties but derives from ScriptableObject
       public string ID;
       public string DisplayName;
       // ...etc
   }
   ```

## Implementation Plan

### Step 1: Update Game.Models.Resource Class

1. Modify the Resource class in Game.Models namespace to directly reference GameConfiguration.ResourceDefinition:

```csharp
namespace Game.Models
{
    [Serializable]
    public class Resource
    {
        public GameConfiguration.ResourceDefinition Definition { get; private set; }
        // ...other properties

        public Resource(GameConfiguration.ResourceDefinition definition)
        {
            Definition = definition;
            Amount = definition.InitialAmount;
            Capacity = definition.InitialCapacity;
            IsUnlocked = definition.VisibleByDefault;
        }

        // ...other methods
    }
}
```

### Step 2: Remove Duplicate ResourceDefinition Classes

1. Remove the ResourceDefinition class from Resource.cs (global namespace)
2. Remove the ResourceDefinition class from ResourceManager.cs (global namespace)
3. Keep only the GameConfiguration.ResourceDefinition class

### Step 3: Update ResourceManager

1. Update the ResourceManager class to use GameConfiguration.ResourceDefinition directly:

```csharp
public class ResourceManager : MonoBehaviour, IResourceManager
{
    [SerializeField] private List<GameConfiguration.ResourceDefinition> resourceDefinitions = new List<GameConfiguration.ResourceDefinition>();
    
    // Runtime resource data
    private Dictionary<string, Game.Models.Resource> resources = new Dictionary<string, Game.Models.Resource>();
    
    // ...other properties and methods

    public void Initialize()
    {
        // Initialize all resources from definitions
        foreach (var definition in resourceDefinitions)
        {
            resources[definition.ID] = new Game.Models.Resource(definition);
        }
        
        // ...other initialization
    }

    // ...other methods
}
```

### Step 4: Update References in Other Classes

1. Find and update all other references to ResourceDefinition to use GameConfiguration.ResourceDefinition
2. Update any type conversions or helper methods that were added previously

### Step 5: Testing

1. Compile the code and fix any remaining errors
2. Test resource-related functionality:
   - Resource initialization
   - Resource production and consumption
   - Resource UI display
   - Resource serialization/deserialization

## Implementation Notes

- Some properties may have different names between versions (BaseCapacity vs InitialCapacity)
- The Game.Models.Resource constructor should properly map between different property names
- This is just the first part of the ambiguity resolution plan, focused solely on ResourceDefinition
- After this is complete, we'll move on to BuildingDefinition and other ambiguous types 