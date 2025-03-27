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

        string costString = "";
        for (int i = 0; i < cost.Count; i++)
        {
            if (i > 0) costString += "\n";
            costString += $"{cost[i].Amount} {cost[i].ResourceID}";
        }
        CostText.text = costString;
    }
}