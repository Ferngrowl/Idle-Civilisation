using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Manages tooltip display and behavior across the game UI
/// </summary>
public class TooltipManager : MonoBehaviour
{
    [Header("Tooltip UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text tooltipTitle;
    [SerializeField] private Text tooltipDescription;
    [SerializeField] private Text tooltipCost;
    [SerializeField] private Text tooltipEffects;
    
    [Header("Tooltip Configuration")]
    [SerializeField] private float edgePadding = 10f;
    [SerializeField] private Vector2 cursorOffset = new Vector2(15f, 15f);
    
    private Canvas parentCanvas;
    private RectTransform tooltipRect;
    
    /// <summary>
    /// Enum for UI element types that can have tooltips
    /// </summary>
    public enum UIType
    {
        Building,
        Upgrade,
        Resource,
        Other
    }
    
    private void Awake()
    {
        // Initialize references if needed
        parentCanvas = GetComponentInParent<Canvas>();
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        
        // Hide tooltip initially
        HideTooltip();
    }
    
    private void Update()
    {
        // Position tooltip at mouse if visible
        if (tooltipPanel.activeSelf)
        {
            PositionTooltipAtMouse();
        }
        
        // Optional: Check for UI elements under cursor that need tooltips
        // CheckForHover();
    }
    
    /// <summary>
    /// Shows the tooltip with the appropriate content based on target type
    /// </summary>
    /// <param name="targetID">ID of the target building, upgrade, etc.</param>
    /// <param name="type">Type of UI element</param>
    /// <param name="message">Optional custom message to display instead</param>
    public void ShowTooltip(string targetID, UIType type, string message = null)
    {
        // Use custom message if provided
        if (!string.IsNullOrEmpty(message))
        {
            tooltipTitle.text = "Info";
            tooltipDescription.text = message;
            tooltipCost.text = string.Empty;
            tooltipEffects.text = string.Empty;
        }
        else
        {
            // Otherwise, fill based on type
            switch (type)
            {
                case UIType.Building:
                    // TODO: Implement building tooltip content
                    break;
                    
                case UIType.Upgrade:
                    // TODO: Implement upgrade tooltip content
                    break;
                    
                case UIType.Resource:
                    // TODO: Implement resource tooltip content
                    break;
                    
                case UIType.Other:
                default:
                    // Generic tooltip
                    tooltipTitle.text = "Info";
                    tooltipDescription.text = "No information available.";
                    tooltipCost.text = string.Empty;
                    tooltipEffects.text = string.Empty;
                    break;
            }
        }
        
        // Show tooltip and position it
        tooltipPanel.SetActive(true);
        PositionTooltipAtMouse();
    }
    
    /// <summary>
    /// Positions the tooltip next to the mouse cursor, ensuring it stays within screen bounds
    /// </summary>
    private void PositionTooltipAtMouse()
    {
        // Get mouse position
        Vector2 mousePos = Input.mousePosition;
        
        // Convert to canvas space if needed
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera || 
            parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                mousePos, parentCanvas.worldCamera, 
                out Vector2 localPos);
                
            tooltipRect.localPosition = localPos + cursorOffset;
        }
        else
        {
            tooltipRect.position = mousePos + cursorOffset;
        }
        
        // Ensure tooltip stays within screen bounds
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        
        // Check right edge
        float rightEdgeDistance = Screen.width - corners[2].x;
        if (rightEdgeDistance < edgePadding)
        {
            tooltipRect.position += new Vector3(rightEdgeDistance - edgePadding, 0, 0);
        }
        
        // Check bottom edge
        float bottomEdgeDistance = corners[0].y;
        if (bottomEdgeDistance < edgePadding)
        {
            tooltipRect.position += new Vector3(0, edgePadding - bottomEdgeDistance, 0);
        }
        
        // Check top edge
        float topEdgeDistance = Screen.height - corners[1].y;
        if (topEdgeDistance < edgePadding)
        {
            tooltipRect.position += new Vector3(0, topEdgeDistance - edgePadding, 0);
        }
    }
    
    /// <summary>
    /// Hides the tooltip
    /// </summary>
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Get formatted string of building effects for display
    /// </summary>
    /// <param name="building">Building definition to show effects for</param>
    /// <returns>Formatted effects string</returns>
    public string GetBuildingEffectsString(BuildingDefinition building)
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
    public string GetUpgradeEffectsString(UpgradeDefinition upgrade)
    {
        string effects = string.Empty;
        
        // TODO: Implement upgrade effects formatting
        
        return effects;
    }
    
    /// <summary>
    /// Check for UI elements under cursor that require tooltips
    /// </summary>
    private void CheckForHover()
    {
        // TODO: Implement hover detection for tooltips
    }
} 