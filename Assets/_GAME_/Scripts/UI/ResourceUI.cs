using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    public string ResourceID;
    public TMP_Text NameText;
    public TMP_Text ValueText;
    public TMP_Text RateText;
    public TMP_Text TimeUntilFullText;
    public Image Icon;
    
    public void UpdateResourceValue(float amount, float capacity, float productionRate)
    {
        // Format the resource amount display using DataFormatter
        if (capacity > 0)
        {
            ValueText.text = $"{DataFormatter.FormatResourceAmount(amount)}/{DataFormatter.FormatResourceAmount(capacity)}";
        }
        else
        {
            ValueText.text = DataFormatter.FormatResourceAmount(amount);
        }
        
        // Format production rate
        string ratePrefix = productionRate >= 0 ? "+" : "";
        RateText.text = $"{ratePrefix}{DataFormatter.FormatResourceAmount(productionRate, true)}/s";
        RateText.color = productionRate >= 0 ? Color.green : Color.red;
    }
    
    public void UpdateTimeDisplay(float timeUntilFull, bool isFilling)
    {
        if (timeUntilFull <= 0) 
        {
            TimeUntilFullText.text = "";
            return;
        }
        
        string prefix = isFilling ? "Full in: " : "Empty in: ";
        TimeUntilFullText.text = $"{prefix}{DataFormatter.FormatTime(timeUntilFull, isFilling)}";
    }
}