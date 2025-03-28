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
    
    public void SetButtonCost(List<ResourceAmount> cost)
    {
        if (cost == null || cost.Count == 0)
        {
            CostText.text = "Free";
            return;
        }

        // Use the DataFormatter utility to format the cost string
        CostText.text = DataFormatter.GetCostString(cost).Replace("Cost:", "");
    }
}