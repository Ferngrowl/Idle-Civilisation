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