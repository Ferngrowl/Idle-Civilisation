# Ambiguity Resolution Implementation Plan

## The Problem

Our codebase has multiple definitions of the same classes in different namespaces:

1. **ResourceDefinition** exists in:
   - Global namespace
   - GameConfiguration namespace

2. **Resource** exists in:
   - Global namespace
   - Game.Models namespace

3. **BuildingDefinition/BuildingData** confusion:
   - BuildingDefinition in global namespace
   - BuildingData used in Building.Definition property

4. **UpgradeEffect** and related types have similar issues

## Branching Strategy

1. **Create dedicated branch**:
   ```bash
   git checkout -b ambiguity-resolution
   ```

2. **Work in isolated chunks**:
   - Create sub-branches for each major component if needed
   - Commit frequently with clear messages

3. **Test before merging**:
   - Run all game functionality after each major change
   - Only merge when stable

## Implementation Steps

### Phase 1: Canonical Type Definitions

1. **Define official locations**:
   - ResourceDefinition → GameConfiguration
   - Resource → Game.Models
   - BuildingDefinition → GameConfiguration
   - Building → Game.Models
   - UpgradeEffect → GameConfiguration

2. **Update class references**:
   - Change Resource.Definition to be GameConfiguration.ResourceDefinition
   - Change Building.Definition to be GameConfiguration.BuildingDefinition

### Phase 2: Remove Duplicates

1. **ResourceDefinition cleanup**:
   - Remove global namespace version
   - Update all references to use GameConfiguration.ResourceDefinition

2. **Building cleanup**:
   - Consolidate BuildingData and BuildingDefinition
   - Update Building class to reference the canonical type

3. **UpgradeEffect cleanup**:
   - Move to GameConfiguration namespace
   - Remove duplicates
   - Update references

### Phase 3: Fix Manager Implementations

1. **Update ResourceManager.cs**:
   - Use fully qualified types for all ambiguous references
   - Fix the Initialize method to create proper Game.Models.Resource instances

2. **Update BuildingManager.cs**:
   - Use fully qualified types for all ambiguous references
   - Fix the GetBuildingDefinition method to return correct type

3. **Update UpgradeManager.cs**:
   - Fix any ambiguity with UpgradeEffect and EffectType

### Phase 4: Testing

1. **Verify compilation**:
   - Fix any remaining compiler errors
   - Ensure all ambiguity warnings are gone

2. **Test functionality**:
   - Resource production
   - Building construction
   - Upgrade purchases
   - Save/load system

## Implementation Notes

- **Be methodical**: Focus on one type at a time
- **Use find-and-replace**: For systematic replacement of types
- **Keep a changelog**: Document all changes made
- **Backup before merging**: Create a backup of the working codebase

## After Implementation

Once all ambiguity issues are resolved:
1. Update documentation
2. Add unit tests to protect against regression
3. Consider adding static analysis tools to prevent reintroduction of ambiguity 