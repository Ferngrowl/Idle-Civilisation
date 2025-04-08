# UpgradeEffect Ambiguity Resolution Plan

## Current Situation

We were encountering ambiguity issues with UpgradeEffect and EffectType in multiple places:

```csharp
// Error example:
if (effect.Type == EffectType.ProductionMultiplier && effect.TargetID == resourceID)
// Ambiguity between 'UpgradeEffect.Type' and 'UpgradeEffect.Type'
// Ambiguity between 'EffectType.ProductionMultiplier' and 'EffectType.ProductionMultiplier'
```

This suggested multiple definitions of UpgradeEffect and EffectType in different namespaces, similar to our ResourceDefinition issue.

## Implementation Progress

### ✅ Step 1: Located All Definitions

We found multiple definitions of these types:

- In UpgradeManager.cs (global namespace):
  ```csharp
  public enum EffectType { UnlockBuilding, UnlockUpgrade, UnlockResource, ... }
  public class UpgradeEffect { ... }
  ```

- In GameConfiguration namespace (ScriptableObjects/UpgradeDefinition.cs):
  ```csharp
  public enum EffectType { ProductionMultiplier, ConsumptionReduction, ... }
  public class UpgradeEffect { ... }
  ```

### ✅ Step 2: Chose Canonical Definitions

We selected:
- `GameConfiguration.UpgradeEffect` as the canonical definition
- `GameConfiguration.EffectType` as the canonical definition for the enum

### ✅ Step 3: Updated All References

1. Updated all references to UpgradeEffect to use GameConfiguration.UpgradeEffect
2. Updated all references to EffectType to use GameConfiguration.EffectType
3. Used integer casts to handle the different enum values between versions

### ✅ Step 4: Updated Core Classes

1. Updated Game.Models.Upgrade to use GameConfiguration.UpgradeDefinition directly
2. Fixed property mismatches (Prerequisites vs RequiredUpgrades)
3. Fixed visibility conditions to check against the proper properties

### ✅ Step 5: Fixed Serialization Issues

Updated UpgradeSaveData class to properly handle the serialization of upgrades.

## Remaining Issues

1. ⚠️ The two EffectType enums have different values, requiring int casting for conversion
2. ⚠️ Some property name differences between versions (Prerequisites vs RequiredUpgrades)
3. ⚠️ Need to test upgrade functionality to verify it works with the consolidated types

## Testing Plan

Test all upgrade-related functionality:
1. Purchasing upgrades
2. Effects of upgrades on production/consumption
3. Upgrade costs and requirements
4. Upgrade UI display
5. Save/load of upgrade data

## Next Steps

1. Address the BuildingManager ambiguity issues using a similar approach
2. Comprehensive testing of all game systems
3. Consider aligning the enum values in future updates to avoid casting 