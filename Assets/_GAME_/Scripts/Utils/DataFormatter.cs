using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class for formatting various data types used in the game
/// </summary>
public static class DataFormatter
{
    /// <summary>
    /// Formats a time value into a human-readable string
    /// </summary>
    /// <param name="time">Time in seconds</param>
    /// <param name="isPositive">Whether the time represents a positive event</param>
    /// <returns>Formatted time string</returns>
    public static string FormatTime(float time, bool isPositive)
    {
        if (time <= 0) return "";
        float absTime = Mathf.Abs(time);
        TimeSpan t = TimeSpan.FromSeconds(absTime);
        
        if (absTime < 60) // Less than a minute
            return $"{(int)absTime}s";
        else if (absTime < 3600) // Less than an hour
            return $"{t.Minutes:D2}m:{t.Seconds:D2}s";
        else if (absTime < 86400) // Less than a day
            return $"{t.Hours:D2}h:{t.Minutes:D2}m";
        else // Days and hours
            return $"{(int)t.TotalDays}d {t.Hours}h";
    }
    
    /// <summary>
    /// Formats a resource amount for display
    /// </summary>
    /// <param name="amount">Resource amount</param>
    /// <param name="useDecimals">Whether to include decimal places</param>
    /// <returns>Formatted amount string</returns>
    public static string FormatResourceAmount(float amount, bool useDecimals = false)
    {
        if (useDecimals)
            return $"{amount:F1}";
        else
            return $"{Mathf.Floor(amount)}";
    }
    
    /// <summary>
    /// Creates a cost string from a list of resource amounts
    /// </summary>
    /// <param name="costs">List of resource costs</param>
    /// <returns>Formatted cost string</returns>
    public static string GetCostString(List<ResourceAmount> costs)
    {
        string costText = "Cost:";
        foreach (var costItem in costs)
        {
            ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(costItem.ResourceID);
            string resourceName = resource != null ? resource.DisplayName : costItem.ResourceID;
            costText += $"\n{resourceName}: {FormatResourceAmount(costItem.Amount)}";
        }
        return costText;
    }
    
    /// <summary>
    /// Creates an effect description string from a list of upgrade effects
    /// </summary>
    /// <param name="effects">List of upgrade effects</param>
    /// <returns>Formatted effects string</returns>
    public static string GetEffectsString(List<UpgradeEffect> effects)
    {
        string effectsText = "Effects:";
        
        if (effects == null || effects.Count == 0)
        {
            return "Effects:\nNone";
        }
        
        foreach (var effect in effects)
        {
            switch (effect.Type)
            {
                case EffectType.ProductionMultiplier:
                    ResourceDefinition prodResource = GameManager.Instance.Resources.GetVisibleResources()
                        .Find(r => r.Definition.ID == effect.TargetID)?.Definition;
                    string prodResourceName = prodResource != null ? prodResource.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {prodResourceName} production";
                    break;
                case EffectType.BuildingProductionMultiplier:
                    BuildingDefinition buildingDef = GameManager.Instance.Buildings.GetBuildingDefinition(effect.TargetID);
                    string buildingName = buildingDef != null ? buildingDef.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {buildingName} production";
                    break;
                case EffectType.ResourceCapacityMultiplier:
                case EffectType.StorageMultiplier:
                    ResourceDefinition capResource = GameManager.Instance.Resources.GetVisibleResources()
                       .Find(r => r.Definition.ID == effect.TargetID)?.Definition;
                    string capResourceName = capResource != null ? capResource.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {capResourceName} capacity";
                    break;
                case EffectType.ConsumptionReduction:
                    ResourceDefinition consumptionResource = GameManager.Instance.Resources.GetVisibleResources()
                       .Find(r => r.Definition.ID == effect.TargetID)?.Definition;
                    string consumptionResourceName = consumptionResource != null ? consumptionResource.DisplayName : effect.TargetID;
                    effectsText += $"\n-{(1 - effect.Value) * 100:F0}% {consumptionResourceName} consumption";
                    break;
                case EffectType.UnlockBuilding:
                    BuildingDefinition unlockBuildingDef = GameManager.Instance.Buildings.GetBuildingDefinition(effect.TargetID);
                    string unlockBuildingName = unlockBuildingDef != null ? unlockBuildingDef.DisplayName: effect.TargetID;
                    effectsText += $"\nUnlocks {unlockBuildingName}";
                    break;
                case EffectType.UnlockUpgrade:
                    UpgradeDefinition unlockUpgradeDef = GameManager.Instance.Upgrades.GetUpgradeDefinition(effect.TargetID);
                    string unlockUpgradeName = unlockUpgradeDef != null ? unlockUpgradeDef.DisplayName : effect.TargetID;
                    effectsText += $"\nUnlocks {unlockUpgradeName}";
                    break;
                case EffectType.UnlockResource:
                    ResourceDefinition unlockResource = GameManager.Instance.Resources.GetResourceDefinition(effect.TargetID);
                    string unlockResourceName = unlockResource != null ? unlockResource.DisplayName : effect.TargetID;
                    effectsText += $"\nUnlocks {unlockResourceName}";
                    break;
                default:
                    effectsText += $"\nUnknown effect type: {effect.Type}";
                    break;
            }
        }
        
        return effectsText;
    }
} 