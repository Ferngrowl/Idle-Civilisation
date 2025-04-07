# Idle Civilization Refactoring Plan

## Current Issues
- Building class is defined in multiple places
- ResourceDefinition is defined in multiple places
- Multiple inconsistent interfaces
- Serialization models don't match interfaces

## Namespace Structure Implementation

We've implemented the following namespace structure:

- `GameConfiguration` - Contains ScriptableObject definitions
  - BuildingDefinition.cs
  - ResourceDefinition.cs
  - UpgradeDefinition.cs (to be created)

- `Game.Models` - Contains runtime game objects
  - Building.cs
  - Resource.cs
  - Upgrade.cs (to be created)

- `Game.Interfaces` - Contains all interfaces
  - IBuildingManager.cs
  - IResourceManager.cs
  - IUpgradeManager.cs
  - ITimeManager.cs
  - IUIManager.cs

- `Serialization` - Contains save data models
  - ResourceSaveData
  - BuildingSaveData
  - UpgradeSaveData
  - TimeSaveData

## Manager Implementation Update Plan

1. Update `ResourceManager.cs`:
   - Add proper namespace imports
   - Remove duplicate Resource and ResourceDefinition classes
   - Update serialization methods

2. Update `BuildingManager.cs`:
   - Add proper namespace imports
   - Remove duplicate Building class
   - Fix serialization methods

3. Update `UpgradeManager.cs`:
   - Add proper namespace imports
   - Create and use Upgrade class from Game.Models
   - Fix serialization methods

4. Update `GameManager.cs`:
   - Update SaveData class to use proper types
   - Fix serialization/deserialization methods

5. Update all managers to use ServiceLocator pattern consistently:
   - Register services in GameManager
   - Use ServiceLocator.Get<T>() to retrieve services

## Implementation Steps

1. Update managers one by one
2. Test save/load functionality
3. Fix any remaining compiler errors
4. Verify game functionality still works

## Type Mapping

| Old Type | New Type |
|----------|----------|
| ResourceData | Serialization.ResourceSaveData |
| BuildingData | GameConfiguration.BuildingDefinition |
| Building | Game.Models.Building |
| ResourceDefinition | GameConfiguration.ResourceDefinition |
| Resource | Game.Models.Resource | 