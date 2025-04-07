# Next Steps

## Current Status

We're in the final stages of the refactoring process. The primary managers have been updated to use interfaces, and the serialization models have been consolidated. We've updated the interface definitions for both IBuildingManager and IUpgradeManager, but we still have ambiguity issues with ResourceDefinition in the UIManager.

## Immediate Tasks

1. **Fix UIManager Implementation (Ambiguity Workaround)**
   - Identified issue: There are multiple `ResourceDefinition` classes in different namespaces
   - Short-term workaround: 
     - Cast to specific types when needed
     - Use explicit variable types
     - Comment out or simplify problematic sections
   - Long-term solution: 
     - Plan future refactoring to consolidate duplicate classes
     - Consider creating a new branch for this major structural change

2. **Test Refactored Codebase**
   - Run the game with the current implementation
   - Verify that basic functionality works despite any remaining ambiguity warnings
   - Test resource production
   - Test building construction
   - Test upgrade purchases
   - Test save/load functionality

## Ambiguity Issues Context

The codebase currently contains multiple definitions of `ResourceDefinition`:
1. A class in the global namespace
2. A class in `GameConfiguration` namespace 
3. Possibly more variants in utility classes

This is causing compiler ambiguity. In a future update, we should consolidate these definitions, but for now, we need a workaround to get the game running.

## Final Validation

1. Ensure the game compiles and runs, even with warnings
2. Verify namespace usage is consistent where possible
3. Check that all interfaces are properly implemented
4. Test save/load functionality thoroughly

## Documentation Updates

1. Update README.md with the new architecture
2. Document the serialization model
3. Document the known ambiguity issues for future developers

## Future Improvements

1. **Remove Duplicate Definitions**: Consolidate ResourceDefinition and other duplicate classes
2. **Unit Tests**: Add unit tests for critical systems
3. **Performance Optimization**: Profile and optimize resource-intensive operations
4. **Code Organization**:
   - Consider moving serialization models into separate files
   - Group manager interfaces logically

## Notes on Serialization

All serialization models are now consolidated in the `SerializationModels.cs` file in the `Serialization` namespace. This provides a single source of truth for all data structures used in saving and loading the game.

## Ambiguity Resolution Progress

We've made progress on the ambiguity resolution plan:

1. ✅ Updated `IResourceManager` interface to use fully qualified type names
2. ✅ Updated `IBuildingManager` interface to use fully qualified type names
3. ⚠️ Updated implementation classes, but there are still ambiguity issues

## Remaining Ambiguity Issues

After updating the interfaces, we're still encountering several ambiguity issues:

1. **In ResourceManager.cs:**
   - Ambiguity between different `ResourceDefinition.ID` properties
   - Ambiguity in `UpgradeEffect.Type`, `UpgradeEffect.TargetID`, and `UpgradeEffect.Value`
   - Conversion issue with Resource initialization

2. **In BuildingManager.cs:**
   - Conversion issues between `BuildingDefinition` and `GameConfiguration.BuildingDefinition`
   - Type mismatch between `Building.Definition` (which is `BuildingData`) and `BuildingDefinition`

## Detailed Ambiguity Resolution Plan

After attempting to fix the ambiguity issues through code edits, it's clear that a more systematic approach is needed. Here's a step-by-step plan:

### 1. Create a Dedicated Branch

Before proceeding, create a new branch specifically for this refactoring:
```bash
git checkout -b ambiguity-resolution
```

### 2. Define Canonical Types

Decide on the official location for each ambiguous type:

| Type | Canonical Namespace | Purpose |
|------|-------------------|---------|
| ResourceDefinition | GameConfiguration | ScriptableObject definition |
| Resource | Game.Models | Runtime instance |
| BuildingDefinition | GameConfiguration | ScriptableObject definition |
| Building | Game.Models | Runtime instance |
| UpgradeEffect | GameConfiguration | Effect definition |
| EffectType | GameConfiguration | Effect type enum |

### 3. Fix Resource Ambiguity

1. Remove all ResourceDefinition classes except in GameConfiguration
2. Update all references to ResourceDefinition to use GameConfiguration.ResourceDefinition
3. Update Resource class in Game.Models to directly reference GameConfiguration.ResourceDefinition

### 4. Fix Building Ambiguity

1. Consolidate BuildingDefinition and BuildingData into one class in GameConfiguration
2. Update Building class to reference GameConfiguration.BuildingDefinition
3. Remove the redundant Building class outside of Game.Models

### 5. Fix Upgrade Ambiguity

1. Move all UpgradeEffect and EffectType definitions to GameConfiguration
2. Remove duplicate definitions
3. Update all references to use GameConfiguration namespace

### 6. Update Manager Implementations

1. Update ResourceManager.cs to use GameConfiguration.ResourceDefinition and Game.Models.Resource
2. Update BuildingManager.cs to use GameConfiguration.BuildingDefinition and Game.Models.Building
3. Update UpgradeManager.cs to use GameConfiguration.UpgradeEffect

### 7. Testing

1. Fix any compiler errors after the refactoring
2. Test all game functionality:
   - Resource production
   - Building construction
   - Upgrades
   - Save/load

### Immediate Next Steps

The most urgent task is to create the branch and start consolidating the ambiguous types. Focus first on ResourceDefinition and Building classes, as they have the most widespread impact.

## Timeline

This is a substantial refactoring that affects core game systems. Plan for:

1. 2-3 hours: Create branch and consolidate ambiguous types
2. 2-3 hours: Update references across the codebase
3. 1-2 hours: Testing and bug fixing

## Fallback Plan

If the refactoring proves too complex or risky in the short term, consider these alternatives:

1. Use preprocessor directives to temporarily disable ambiguous code
2. Create wrapper classes that encapsulate the ambiguous types
3. Focus development on areas that don't rely on ambiguous types until a proper refactoring can be completed

# Progress Update

## Completed:
- ✅ Created necessary namespaces and interfaces
- ✅ Updated ResourceManager, BuildingManager, UpgradeManager, and TimeManager
- ✅ Updated GameManager to use interfaces
- ✅ Consolidated serialization models into a single file
- ✅ Fixed SaveData reference issues
- ✅ Fixed interface definitions with fully qualified types
- ⚠️ Started implementation updates (with remaining ambiguity issues)

## Remaining:
- Fully resolve ambiguity issues with type qualifications
- Test the implementation
- Plan for future comprehensive cleanup

# Next Implementation Steps - Progress Update

## Completed Steps
- ✓ Created namespaces
  - ✓ `

# Ambiguity Resolution Progress Update

## Completed Steps:
1. ✅ Created detailed resolution plans for ResourceDefinition and UpgradeEffect
2. ✅ Updated Resource.cs to remove duplicate ResourceDefinition and reference GameConfiguration.ResourceDefinition
3. ✅ Updated ResourceManager.cs to remove duplicate classes and simplify initialization
4. ✅ Added ResourceSaveData.ResourceData inner class back to fix serialization

## Current Issues:
1. ⚠️ Multiple definitions of UpgradeEffect and EffectType causing ambiguity
2. ⚠️ Converting between different versions of types is problematic
3. ⚠️ BuildingManager has similar issues with BuildingDefinition vs BuildingData

## Next Immediate Actions:

1. **Create a dedicated branch for this refactoring**
   ```bash
   git checkout -b ambiguity-resolution
   ```

2. **Complete ResourceDefinition consolidation**:
   - Verify all references point to GameConfiguration.ResourceDefinition
   - Test resource initialization, updates, and serialization

3. **Address UpgradeEffect ambiguity**:
   - Update any nested classes in UpgradeManager.cs that use UpgradeEffect
   - Focus on resolving the EffectType enum ambiguity
   - Either:
     a) Update all code to use GameConfiguration.EffectType
     b) Temporarily use integer casts to avoid direct enum comparisons

4. **Fix BuildingManager issues**:
   - Similar to ResourceManager, update Building.cs to reference GameConfiguration.BuildingDefinition
   - Remove duplicate BuildingDefinition classes
   - Address BuildingData vs BuildingDefinition inconsistency

## Key Files to Update:
1. `Core/ScriptableObjects/UpgradeDefinition.cs`
2. `Core/ScriptableObjects/BuildingDefinition.cs`
3. `Core/Building.cs`
4. `Core/Upgrade.cs`
5. `Managers/UpgradeManager.cs`
6. `Managers/BuildingManager.cs`

## Testing Plan:
1. After each class is updated, compile and run basic tests
2. Ensure resource production and consumption still works
3. Verify buildings can be constructed and function properly
4. Test that upgrades can be purchased and have the correct effects
5. Test save/load functionality to ensure serialization works