using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class UpgradeUI : MonoBehaviour
{
    public string UpgradeID;
    public TMP_Text NameText;
    public TMP_Text DescriptionText;
    public TMP_Text CostText;
    public Image Icon;
    
    private UIManager uiManager;
    
    /// <summary>
    /// Initialize this UpgradeUI instance with the provided upgrade data
    /// </summary>
    public void Initialize(string upgradeID, string displayName, string description, List<ResourceAmount> cost, Sprite icon = null)
    {
        UpgradeID = upgradeID;
        
        // Set UI text elements
        if (NameText != null) NameText.text = displayName;
        if (DescriptionText != null) DescriptionText.text = description;
        if (Icon != null && icon != null) Icon.sprite = icon;
        
        // Set cost using helper method
        SetButtonCost(cost);
        
        // Set up tooltip
        SetupTooltip();
    }
    
    /// <summary>
    /// Sets up the tooltip functionality for this upgrade UI element
    /// </summary>
    private void SetupTooltip()
    {
        if (string.IsNullOrEmpty(UpgradeID))
            return;
        
        // Find UI manager if not already set
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        
        if (uiManager == null)
            return;
        
        // Add tooltip trigger to the game object
        TooltipTrigger.AddTooltip(gameObject, uiManager, UpgradeID, UIType.Upgrade);
        
        // Make sure buttons within the upgrade UI can be clicked
        ConfigureButtonsForTooltip();
    }
    
    /// <summary>
    /// Ensures buttons within the upgrade UI can still be clicked when tooltips are active
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
}