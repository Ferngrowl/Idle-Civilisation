using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GameConfiguration;

// Assuming TooltipProvider is defined in the project's default namespace
// If it's in a different namespace, use that one

public class BuildingUI : MonoBehaviour
{
    public string BuildingID;
    public TMP_Text NameText;
    public TMP_Text DescriptionText;
    public TMP_Text CountText;
    public TMP_Text CostText;
    public Image Icon;
    
    private UIManager uiManager;
    
    /// <summary>
    /// Initialize this BuildingUI instance with the provided building data
    /// </summary>
    public void Initialize(string buildingID, string displayName, string description, int count, List<ResourceAmount> cost, Sprite icon = null)
    {
        BuildingID = buildingID;
        
        // Set UI text elements
        if (NameText != null) NameText.text = displayName;
        if (DescriptionText != null) DescriptionText.text = description;
        if (CountText != null) CountText.text = $"Owned: {count}";
        if (Icon != null && icon != null) Icon.sprite = icon;
        
        // Set cost using helper method
        SetButtonCost(cost);
        
        // Set up tooltip
        SetupTooltip();
    }
    
    /// <summary>
    /// Sets up the tooltip functionality for this building UI element
    /// </summary>
    private void SetupTooltip()
    {
        if (string.IsNullOrEmpty(BuildingID))
            return;
        
        // Find UI manager if not already set
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        
        if (uiManager == null)
            return;
        
        // Add tooltip trigger to the game object
        TooltipTrigger.AddTooltip(gameObject, uiManager, BuildingID, UIType.Building);
        
        // Make sure buttons remain clickable by ensuring they're not blocked by the tooltip
        ConfigureButtonsForTooltip();
    }
    
    /// <summary>
    /// Ensures buttons within the building UI can still be clicked when tooltips are active
    /// </summary>
    private void ConfigureButtonsForTooltip()
    {
        // Find all buttons in this UI element
        Button[] buttons = GetComponentsInChildren<Button>();
        
        // Make sure each button's raycast target is enabled
        foreach (Button button in buttons)
        {
            if (button != null && button.image != null)
            {
                // Ensure button image is still a raycast target
                button.image.raycastTarget = true;
            }
        }
    }
    
    public void SetButtonCost(List<ResourceAmount> cost)
    {
        if (CostText == null) return;
        
        if (cost == null || cost.Count == 0)
        {
            CostText.text = "Free";
            return;
        }

        // Use the DataFormatter utility to format the cost string
        CostText.text = DataFormatter.GetCostString(cost).Replace("Cost:", "");
    }
    
    /// <summary>
    /// Update the building count display
    /// </summary>
    public void UpdateCount(int count)
    {
        if (CountText != null)
        {
            CountText.text = $"Owned: {count}";
        }
    }
}