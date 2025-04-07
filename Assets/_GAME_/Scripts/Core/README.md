# Core Directory Structure

This directory contains the foundational components and interfaces of the Idle Civilization game.

## Namespaces

### Game.Interfaces
Contains all manager interfaces that define the contracts for game systems:
- `IResourceManager.cs` - Resource management contract
- `IBuildingManager.cs` - Building management contract
- `IUpgradeManager.cs` - Upgrade management contract
- `ITimeManager.cs` - Time system contract
- `IUIManager.cs` - UI system contract

### Game.Models
Contains runtime model classes representing the game state:
- `Building.cs` - Runtime building instance
- `Resource.cs` - Runtime resource instance
- `Upgrade.cs` - Runtime upgrade instance

### GameConfiguration
Contains ScriptableObject configurations in the ScriptableObjects directory:
- `BuildingDefinition.cs` - Building configuration
- `ResourceDefinition.cs` - Resource configuration
- `UpgradeDefinition.cs` - Upgrade configuration

### Serialization
Contains data models for saving/loading game state:
- `SerializationModels.cs` - Contains serializable data types

## Supporting Classes

### ServiceLocator
The `ServiceLocator.cs` provides service location functionality for dependency injection, allowing managers to be decoupled from each other.

## Implementation Notes

When implementing managers that use these interfaces:
1. Use proper namespace imports
2. Implement all interface methods
3. Properly handle serialization with correct types
4. Register services with ServiceLocator

All game systems should follow the Manager pattern and implement the corresponding interface. 