using System;
using UnityEngine;

namespace GameConfiguration
{
    /// <summary>
    /// Definition for a resource type - used for editor configuration
    /// </summary>
    [CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource Definition")]
    public class ResourceDefinition : ScriptableObject
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
        [Tooltip("Icon shown in UI")]
        public Sprite Icon;
        
        [Tooltip("Whether this resource has a capacity limit")]
        public bool HasCapacity = false;
        
        [Tooltip("Initial capacity of the resource")]
        public float InitialCapacity = 100f;
        
        [Tooltip("Initial amount of the resource")]
        public float InitialAmount = 0f;
        
        [Header("Visibility")]
        [Tooltip("Whether this resource is visible from the start")]
        public bool VisibleByDefault = false;
    }
} 