using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Game.Interfaces;
using GameConfiguration;

/// <summary>
/// Manages global tooltip system for the game
/// </summary>
public class TooltipManager : MonoBehaviour
{
    [Header("Tooltip Components")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private TMP_Text tooltipCost;
    [SerializeField] private TMP_Text tooltipEffects;
    
    [Header("Visual Settings")]
    [SerializeField] private float edgePadding = 10f;
    [SerializeField] private Vector2 cursorOffset = new Vector2(15, -15);
    
    // State tracking
    private bool isTooltipActive = false;
    private IUIManager uiManager;
    
    /// <summary>
    /// Tooltip UI element types
    /// </summary>
    public enum UIType 
    {
        Building,
        Upgrade,
        Resource
    }
    
    #region Unity Lifecycle
    
    // For position stability
    private Vector2 lastMousePosition;
    private bool allowPositionUpdate = true;

    private void Awake()
    {
        // Ensure tooltip is hidden at start
        HideTooltip();
        lastMousePosition = Input.mousePosition;
        
        // Get dependencies 
        uiManager = ServiceLocator.Get<IUIManager>();
    }
    
    private void Update()
    {
        // Update tooltip position if active, but add stabilization
        if (isTooltipActive && tooltipPanel && tooltipPanel.activeSelf)
        {
            // Check if mouse has moved significantly
            if (Vector2.Distance(lastMousePosition, Input.mousePosition) > 3f)
            {
                allowPositionUpdate = true;
                lastMousePosition = Input.mousePosition;
            }
            
            // Only update position when needed to reduce flickering
            if (allowPositionUpdate)
            {
                PositionTooltipAtMouse();
                allowPositionUpdate = false;
            }
        }
    }
    
    #endregion
    
    #region Public API
    
    // Add debouncing to prevent rapid show/hide cycles
    private float lastActivationTime = 0f;
    private const float ACTIVATION_DEBOUNCE = 0.1f;

    /// <summary>
    /// Shows tooltip with specified content
    /// </summary>
    /// <param name="targetID">ID of building/upgrade/resource</param>
    /// <param name="type">Type of UI element</param>
    /// <param name="message">Optional override message</param>
    public void ShowTooltip(string targetID, UIType type, string message = null)
    {
        // Debounce rapid show/hide cycles
        if (Time.unscaledTime - lastActivationTime < ACTIVATION_DEBOUNCE)
            return;
        
        lastActivationTime = Time.unscaledTime;
        
        // Clear and activate tooltip
        ClearTooltipContent();
        ActivateTooltip();
        
        // Set content based on type or message
        if (!string.IsNullOrEmpty(message))
        {
            SetSimpleTooltipContent("Info", message);
        }
        else
        {
            // Get content from appropriate manager
            if (uiManager != null)
            {
                uiManager.ShowTooltip(targetID, (global::UIType)type);
            }
        }
    }
    
    /// <summary>
    /// Hides the tooltip
    /// </summary>
    public void HideTooltip(bool immediate = true)
    {
        // Debounce rapid show/hide cycles
        if (Time.unscaledTime - lastActivationTime < ACTIVATION_DEBOUNCE)
            return;
        
        lastActivationTime = Time.unscaledTime;
        
        if (immediate)
            DeactivateTooltip();
        else
            ClearTooltipContent();
            
        isTooltipActive = false;
    }
    
    /// <summary>
    /// Returns whether tooltip is currently shown
    /// </summary>
    public bool IsTooltipActive()
    {
        return isTooltipActive;
    }
    
    #endregion
    
    #region Internal Methods
    
    /// <summary>
    /// Activates tooltip panel
    /// </summary>
    private void ActivateTooltip()
    {
        if (tooltipPanel)
        {
            tooltipPanel.SetActive(true);
            isTooltipActive = true;
            FormatTooltip();
            PositionTooltipAtMouse();
        }
    }
    
    /// <summary>
    /// Deactivates tooltip panel
    /// </summary>
    private void DeactivateTooltip()
    {
        if (tooltipPanel)
            tooltipPanel.SetActive(false);
            
        isTooltipActive = false;
    }
    
    /// <summary>
    /// Clears all tooltip text fields
    /// </summary>
    private void ClearTooltipContent()
    {
        if (tooltipTitle)
            tooltipTitle.text = string.Empty;
            
        if (tooltipDescription)
            tooltipDescription.text = string.Empty;
            
        if (tooltipCost)
            tooltipCost.text = string.Empty;
            
        if (tooltipEffects)
            tooltipEffects.text = string.Empty;
    }
    
    /// <summary>
    /// Sets basic tooltip content
    /// </summary>
    private void SetSimpleTooltipContent(string title, string description)
    {
        if (tooltipTitle)
            tooltipTitle.text = title;
            
        if (tooltipDescription)
            tooltipDescription.text = description;
    }
    
    /// <summary>
    /// Formats tooltip text for display
    /// </summary>
    private void FormatTooltip()
    {
        // Enable text wrapping
        EnableTextWrapping();
        
        // Ensure tooltip has a dark background for better text visibility
        EnsureTooltipBackground();
        
        // Update layout
        if (tooltipPanel)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());
        }
    }
    
    /// <summary>
    /// Ensures tooltip has a proper background for text visibility
    /// </summary>
    private void EnsureTooltipBackground()
    {
        if (!tooltipPanel) return;
        
        // Set minimum size
        LayoutElement layout = tooltipPanel.GetComponent<LayoutElement>();
        if (layout == null)
            layout = tooltipPanel.AddComponent<LayoutElement>();
        
        layout.minWidth = 200;
        layout.minHeight = 100;
        
        // Add dark background
        Image background = tooltipPanel.GetComponent<Image>();
        if (background == null)
            background = tooltipPanel.AddComponent<Image>();
        
        // Dark grey semi-transparent for better readability
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    }
    
    /// <summary>
    /// Enables text wrapping on all tooltip text components
    /// </summary>
    private void EnableTextWrapping()
    {
        if (tooltipDescription is TMP_Text tmpDesc)
            tmpDesc.enableWordWrapping = true;
            
        if (tooltipEffects is TMP_Text tmpEffects)
            tmpEffects.enableWordWrapping = true;
            
        if (tooltipCost is TMP_Text tmpCost)
            tmpCost.enableWordWrapping = true;
    }
    
    /// <summary>
    /// Positions tooltip at mouse cursor with screen bounds checking
    /// </summary>
    private void PositionTooltipAtMouse()
    {
        if (!tooltipPanel)
            return;
            
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (!tooltipRect)
            return;
        
        // Get dimensions and position
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 mousePos = Input.mousePosition;
        Vector2 position = mousePos + cursorOffset;
        
        // Adjust for screen edges
        AdjustPositionForScreenBounds(ref position, mousePos, tooltipSize);
        
        // Apply position
        tooltipRect.position = position;
    }
    
    /// <summary>
    /// Adjusts position to stay within screen bounds
    /// </summary>
    private void AdjustPositionForScreenBounds(ref Vector2 position, Vector2 mousePos, Vector2 tooltipSize)
    {
        // Right edge
        if (position.x + tooltipSize.x > Screen.width - edgePadding)
            position.x = mousePos.x - tooltipSize.x - cursorOffset.x;
        
        // Bottom edge
        if (position.y + tooltipSize.y > Screen.height - edgePadding)
            position.y = mousePos.y - tooltipSize.y - cursorOffset.y;
        
        // Left edge
        if (position.x < edgePadding)
            position.x = edgePadding;
        
        // Top edge
        if (position.y < edgePadding)
            position.y = edgePadding;
    }
    
    #endregion
    
    /// <summary>
    /// Get formatted string of building effects for display
    /// </summary>
    /// <param name="building">Building definition to show effects for</param>
    /// <returns>Formatted effects string</returns>
    public string GetBuildingEffectsString(GameConfiguration.BuildingDefinition building)
    {
        string effects = string.Empty;
        
        // TODO: Implement building effects formatting
        
        return effects;
    }
    
    /// <summary>
    /// Get formatted string of upgrade effects for display
    /// </summary>
    /// <param name="upgrade">Upgrade definition to show effects for</param>
    /// <returns>Formatted effects string</returns>
    public string GetUpgradeEffectsString(GameConfiguration.UpgradeDefinition upgrade)
    {
        string effects = string.Empty;
        
        // TODO: Implement upgrade effects formatting
        
        return effects;
    }
} 