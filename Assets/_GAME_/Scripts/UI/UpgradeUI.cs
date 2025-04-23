using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GameConfiguration;


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