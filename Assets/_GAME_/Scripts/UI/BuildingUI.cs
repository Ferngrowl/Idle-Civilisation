using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingUI : MonoBehaviour
{
    public string BuildingID;
    public TMP_Text NameText;
    public TMP_Text DescriptionText;
    public TMP_Text CountText;
    public TMP_Text CostText;
    public Image Icon;
    
    public void SetButtonCost(List<ResourceAmount> cost) { /* implementation */ }
}