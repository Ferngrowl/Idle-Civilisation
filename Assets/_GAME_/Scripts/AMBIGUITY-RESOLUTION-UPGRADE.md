# UpgradeEffect Ambiguity Resolution Plan

## Current Situation

We're encountering ambiguity issues with UpgradeEffect and EffectType in multiple places:

```csharp
// Error example:
if (effect.Type == EffectType.ProductionMultiplier && effect.TargetID == resourceID)
// Ambiguity between 'UpgradeEffect.Type' and 'UpgradeEffect.Type'
// Ambiguity between 'EffectType.ProductionMultiplier' and 'EffectType.ProductionMultiplier'
```

This suggests there are multiple definitions of UpgradeEffect and EffectType in different namespaces, similar to our ResourceDefinition issue.

## Investigation Required

We need to locate all definitions of these types:

1. Look for UpgradeEffect in:
   - Global namespace
   - GameConfiguration namespace
   - Other potential namespaces

2. Look for EffectType enum in:
   - Global namespace
   - GameConfiguration namespace
   - Other potential namespaces

## Implementation Plan

### Step 1: Locate All Definitions

Search the codebase for all definitions of UpgradeEffect and EffectType.

### Step 2: Choose Canonical Definitions

Based on our overall strategy:
- GameConfiguration.UpgradeEffect should be the canonical definition
- GameConfiguration.EffectType should be the canonical definition for the enum

### Step 3: Update All References

1. Update all references to UpgradeEffect to use GameConfiguration.UpgradeEffect
2. Update all references to EffectType to use GameConfiguration.EffectType
3. Fully qualify all references to enum values:

```csharp
// Change from:
if (effect.Type == EffectType.ProductionMultiplier)

// To:
if (effect.Type == GameConfiguration.EffectType.ProductionMultiplier)
```

### Step 4: Update ResourceManager

Update the ResourceManager to properly handle these types:

```csharp
// In ResourceManager.cs:
private float CalculateProductionRate(string resourceID)
{
    // ... existing code ...
    
    // Apply upgrades
    float multiplier = 1f;
    foreach (var upgrade in upgradeManager.GetAllPurchasedUpgrades())
    {
        foreach (var effect in upgrade.Definition.Effects)
        {
            if (effect.Type == GameConfiguration.EffectType.ProductionMultiplier && 
                effect.TargetID == resourceID)
            {
                multiplier *= (1f + effect.Value);
            }
        }
    }
    
    return baseProduction * multiplier;
}
```

Repeat for CalculateConsumptionRate and any other methods using these types.

### Step 5: Update UpgradeManager

Update the UpgradeManager to use the canonical types consistently.

### Step 6: Update Upgrade Model Classes

Similar to what we did with Resource, update any Upgrade model classes to reference the canonical types.

### Step 7: Remove Duplicate Definitions

Once all references are updated, remove the duplicate definitions of UpgradeEffect and EffectType.

## Testing

Test all upgrade-related functionality after these changes:
1. Purchasing upgrades
2. Effects of upgrades on production/consumption
3. Upgrade costs and requirements
4. Upgrade UI display
5. Save/load of upgrade data 