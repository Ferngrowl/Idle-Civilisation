using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a seasonal effect on a resource
/// </summary>
[Serializable]
public class SeasonEffect
{
    public int Season;
    public int Weather;
    public float ProductionModifier = 1f;
}

/// <summary>
/// ScriptableObject containing resource configuration data
/// </summary>
[CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource Definition")]
public class ResourceData : ScriptableObject
{
    [Header("Resource Identity")]
    [Tooltip("Unique identifier for the resource")]
    public string ID;
    
    [Tooltip("Display name shown in UI")]
    public string DisplayName;
    
    [Tooltip("Description of the resource")]
    [TextArea(2, 5)]
    public string Description;
    
    [Header("Resource Properties")]
    [Tooltip("Whether this resource has a storage limit")]
    public bool HasCapacity = true;
    
    [Tooltip("Base storage capacity before buildings and upgrades")]
    public float BaseCapacity = 100f;
    
    [Tooltip("Initial amount of this resource when starting a new game")]
    public float InitialAmount = 0f;
    
    [Tooltip("Whether this resource is visible from the start")]
    public bool VisibleByDefault = false;
    
    [Header("Seasonal Effects")]
    [Tooltip("How seasons affect this resource's production")]
    public List<SeasonEffect> SeasonalEffects = new List<SeasonEffect>();
    
    [Header("UI Display")]
    [Tooltip("Format string for displaying resource amount")]
    public string DisplayFormat = "0.0";
    
    [Tooltip("Icon for the resource")]
    public Sprite Icon;
    
    [Tooltip("Color for the resource in UI")]
    public Color Color = Color.white;
} 