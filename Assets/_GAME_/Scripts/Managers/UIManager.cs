using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manages all UI elements and interactions in the game. Handles resources display, building lists,
/// upgrade panels, and tooltips.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Resource UI")]
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private float resourceUpdateInterval = 0.25f;
    private float resourceUpdateTimer;
    private Dictionary<string, ResourceUI> resourceUIElements = new Dictionary<string, ResourceUI>();
    
    [Header("Building UI")]
    [SerializeField] private Transform buildingContainer;
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private TMP_Text buildingTabButton;
    [SerializeField] private GameObject buildingPanel;
    
    [Header("Upgrade UI")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private TMP_Text upgradeTabButton;
    [SerializeField] private GameObject upgradePanel;
    
    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private TMP_Text tooltipCost;
    [SerializeField] private TMP_Text tooltipEffects;
    
    // UI State tracking
    private UITab currentTab = UITab.Buildings;
    
    /// <summary>
    /// Initialize UI elements and setup event listeners
    /// </summary>
    public void Initialize()
    {
        // Setup initial UI state
        ShowTab(UITab.Buildings);
        
        // Clear any template elements
        ClearContainer(resourceContainer);
        ClearContainer(buildingContainer);
        ClearContainer(upgradeContainer);
        
        // Hide tooltip on start
        HideTooltip();
        
        // Create UI for initially visible resources
        RefreshResourceView();
        
        // Create UI for buildings and upgrades
        RefreshBuildingView();
        RefreshUpgradeView();
    }
    
    private void Update()
    {
        // Only update resource UI at specified intervals to prevent UI thrashing
        resourceUpdateTimer += Time.deltaTime;
        if (resourceUpdateTimer >= resourceUpdateInterval)
        {
            UpdateResourceValues();
            resourceUpdateTimer = 0f;
            
            // Also update building buttons (for affordability)
            UpdateBuildingButtons();
        }
    }
    
    #region Tab Management
    
    /// <summary>
    /// Switch between different UI tabs (Buildings, Upgrades, etc.)
    /// </summary>
    public void ShowTab(UITab tab)
    {
        currentTab = tab;
        
        // Hide all panels first
        buildingPanel.SetActive(false);
        upgradePanel.SetActive(false);
        
        // Show selected panel
        switch (tab)
        {
            case UITab.Buildings:
                buildingPanel.SetActive(true);
                break;
            case UITab.Upgrades:
                upgradePanel.SetActive(true);
                break;
        }
        
        // Update tab button states
        UpdateTabButtons();
    }
    
    /// <summary>
    /// Update tab button visual states
    /// </summary>
    private void UpdateTabButtons()
    {
        Color activeColor = new Color(1f, 1f, 1f, 1f);
        Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        buildingTabButton.color = (currentTab == UITab.Buildings) ? activeColor : inactiveColor;
        upgradeTabButton.color = (currentTab == UITab.Upgrades) ? activeColor : inactiveColor;
    }
    
    #endregion
    
    #region Resource UI
    
    /// <summary>
    /// Update the resource display with new values
    /// </summary>
    private void UpdateResourceValues()
    {
        ResourceManager resourceManager = GameManager.Instance.Resources;
        foreach (var resourceUI in resourceUIElements.Values)
        {
            string resourceID = resourceUI.ResourceID;
            float amount = resourceManager.GetAmount(resourceID);
            float capacity = resourceManager.GetCapacity(resourceID);
            
            // Update display
            resourceUI.ValueText.text = capacity > 0 ? $"{Mathf.Floor(amount)}/{Mathf.Floor(capacity)}" : $"{Mathf.Floor(amount)}";
            
            // Production rate
            float productionRate = CalculateNetProductionRate(resourceID);
            string rateText = productionRate >= 0 ? $"+{productionRate:F1}/s" : $"{productionRate:F1}/s";
            resourceUI.RateText.text = rateText;
            resourceUI.RateText.color = productionRate >= 0 ? Color.green : Color.red;
        }
    }
    
    /// <summary>
    /// Calculate the net production rate for a resource (for display)
    /// </summary>
    private float CalculateNetProductionRate(string resourceID)
    {
        // This is a simplified version for UI display - the real calculation happens in ResourceManager
        // We're just showing an approximation for the player.
        float productionRate = GameManager.Instance.Resources.GetProductionRate(resourceID);
        float consumptionRate = GameManager.Instance.Resources.GetConsumptionRate(resourceID);
        return productionRate - consumptionRate;
    }
    
    /// <summary>
    /// Refresh the entire resource view.  Called on initialization and when resources change visibility.
    /// </summary>
    public void RefreshResourceView()
    {
        ResourceManager resourceManager = GameManager.Instance.Resources;
        List<Resource> visibleResources = resourceManager.GetVisibleResources();
        
        // Clear existing UI elements
        ClearContainer(resourceContainer);
        resourceUIElements.Clear();
        
        // Create UI elements for visible resources
        foreach (var resource in visibleResources)
        {
            GameObject resourceUIObj = Instantiate(resourcePrefab, resourceContainer);
            ResourceUI resourceUI = resourceUIObj.GetComponent<ResourceUI>();
            if (resourceUI != null)
            {
                resourceUI.ResourceID = resource.Definition.ID;
                resourceUI.NameText.text = resource.Definition.DisplayName;
                resourceUI.Icon.sprite = resource.Definition.Icon;
                resourceUIElements.Add(resource.Definition.ID, resourceUI);
            }
            else
            {
                Debug.LogError($"Resource prefab is missing ResourceUI component: {resource.Definition.ID}");
            }
        }
        
        // Initial update of resource values
        UpdateResourceValues();
    }
    
    #endregion
    
    #region Building UI
    
    /// <summary>
    /// Refresh the entire building view. Called on initialization and when buildings change visibility.
    /// </summary>
    public void RefreshBuildingView()
    {
        BuildingManager buildingManager = GameManager.Instance.Buildings;
        List<Building> visibleBuildings = buildingManager.GetVisibleBuildings();
        
        // Clear existing UI elements
        ClearContainer(buildingContainer);
        
        // Create UI elements for visible buildings
        foreach (var building in visibleBuildings)
        {
            GameObject buildingUIObj = Instantiate(buildingPrefab, buildingContainer);
            BuildingUI buildingUI = buildingUIObj.GetComponent<BuildingUI>();
            buildingUIObj.name = building.Definition.ID; 
            if (buildingUI != null)
            {
                buildingUI.BuildingID = building.Definition.ID;
                buildingUI.NameText.text = building.Definition.DisplayName;
                buildingUI.DescriptionText.text = building.Definition.Description;
                buildingUI.CountText.text = $"Owned: {building.Count}";
                Dictionary<string, float> currentCost = GameManager.Instance.Buildings.CalculateBuildingCost(building.Definition.ID);
                buildingUI.CostText.text = GetBuildingCostString(currentCost);
                buildingUI.SetButtonCost(currentCost);
                buildingUI.Icon.sprite = building.Definition.Icon;
                
                // Set the button's onClick to purchase this building
                Button buyButton = buildingUI.GetComponentInChildren<Button>(); // Ensure you get the Button component.
                if (buyButton != null)
                {
                    buyButton.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance.Buildings.CanConstructBuilding(building.Definition.ID))
                        {
                            GameManager.Instance.Buildings.ConstructBuilding(building.Definition.ID);
                            UpdateBuildingCount(building.Definition.ID); //update count
                            UpdateResourceValues(); //update resources
                            UpdateBuildingButtons();
                            ShowTooltip(building.Definition.ID, UIType.Building); //update tooltip
                        }
                        else
                        {
                            // Optionally, show a message in the tooltip
                            ShowTooltip(building.Definition.ID, UIType.Building, "Can't afford!");
                        }
                    });
                }
                else
                {
                    Debug.LogError($"Building prefab is missing Button component: {building.Definition.ID}");
                }
                
                // Attach the tooltip trigger
                TooltipTrigger trigger = buildingUI.gameObject.AddComponent<TooltipTrigger>();
                trigger.uiManager = this; // Pass UIManager instance
                trigger.TargetID = building.Definition.ID;
                trigger.type = UIType.Building;
            }
            else
            {
                Debug.LogError($"Building prefab is missing BuildingUI component: {building.Definition.ID}");
            }
        }
        UpdateBuildingButtons();
    }
    
    /// <summary>
    /// Update the display of a building's count.
    /// </summary>
    private void UpdateBuildingCount(string buildingID)
    {
        BuildingManager buildingManager = GameManager.Instance.Buildings;
        Building building = buildingManager.GetBuilding(buildingID);
        if (building != null)
        {
            Transform buildingUIElement = buildingContainer.Find(buildingID); //changed
            if (buildingUIElement != null)
            {
                BuildingUI buildingUI = buildingUIElement.GetComponent<BuildingUI>();
                if (buildingUI != null)
                {
                    buildingUI.CountText.text = $"Owned: {building.Count}";
                }
            }
        }
    }
    
    /// <summary>
    /// Update building buttons, specifically their interactability based on resource availability.
    /// </summary>
    private void UpdateBuildingButtons()
    {
        BuildingManager buildingManager = GameManager.Instance.Buildings;
        foreach (Transform child in buildingContainer)
        {
            BuildingUI buildingUI = child.GetComponent<BuildingUI>();
            if (buildingUI != null)
            {
                string buildingID = buildingUI.BuildingID;
                Button buyButton = buildingUI.GetComponentInChildren<Button>();
                if (buyButton != null)
                {
                    buyButton.interactable = buildingManager.CanConstructBuilding(buildingID);                
                }
            }
        }
    }
    
    /// <summary>
    /// Get the cost string.
    /// </summary>
    private string GetBuildingCostString(Dictionary<string, float> cost)
    {
        string costText = "Cost:";
        foreach (var costItem in cost)
        {
            ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources().Find(r => r.Definition.ID == costItem.Key)?.Definition;
            string resourceName = resource != null ? resource.DisplayName : costItem.Key;
            costText += $"\n{resourceName}: {costItem.Value}";
        }
        return costText;
    }
    
    #endregion
    
    #region Upgrade UI
    
    /// <summary>
    /// Refresh the entire upgrade view. Called on initialization and when upgrades change visibility.
    /// </summary>
    public void RefreshUpgradeView()
    {
        UpgradeManager upgradeManager = GameManager.Instance.Upgrades;
        List<Upgrade> visibleUpgrades = upgradeManager.GetVisibleUpgrades();
        
        // Clear existing UI elements
        ClearContainer(upgradeContainer);
        
        // Create UI elements for visible upgrades
        foreach (var upgrade in visibleUpgrades)
        {
            GameObject upgradeUIObj = Instantiate(upgradePrefab, upgradeContainer);
            UpgradeUI upgradeUI = upgradeUIObj.GetComponent<UpgradeUI>();
            if (upgradeUI != null)
            {
                upgradeUI.UpgradeID = upgrade.Definition.ID;
                upgradeUI.NameText.text = upgrade.Definition.DisplayName;
                upgradeUI.DescriptionText.text = upgrade.Definition.Description;
                upgradeUI.CostText.text = GetUpgradeCostString(upgrade.Definition.Cost);
                upgradeUI.SetButtonCost(upgrade.Definition.Cost);
                upgradeUI.Icon.sprite = upgrade.Definition.Icon;
                
                // Set the button's onClick to purchase this upgrade
                Button buyButton = upgradeUI.GetComponentInChildren<Button>();
                if (buyButton != null)
                {
                    buyButton.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance.Upgrades.CanPurchaseUpgrade(upgrade.Definition.ID))
                        {
                            GameManager.Instance.Upgrades.PurchaseUpgrade(upgrade.Definition.ID);
                            UpdateUpgradeButtons(); // Update button states after purchase
                            RefreshUpgradeView();
                            UpdateResourceValues();
                            ShowTooltip(upgrade.Definition.ID, UIType.Upgrade);
                        }
                        else
                        {
                            ShowTooltip(upgrade.Definition.ID, UIType.Upgrade, "Can't afford!");
                        }
                    });
                }
                else
                {
                    Debug.LogError($"Upgrade prefab is missing Button component: {upgrade.Definition.ID}");
                }
                
                // Attach the tooltip trigger.
                TooltipTrigger trigger = upgradeUI.gameObject.AddComponent<TooltipTrigger>();
                trigger.uiManager = this;  // Pass UIManager instance
                trigger.TargetID = upgrade.Definition.ID;
                trigger.type = UIType.Upgrade;
            }
            else
            {
                Debug.LogError($"Upgrade prefab is missing UpgradeUI component: {upgrade.Definition.ID}");
            }
        }
        UpdateUpgradeButtons();
    }
    
    /// <summary>
    /// Updates the Upgrade buttons.
    /// </summary>
    private void UpdateUpgradeButtons()
    {
        UpgradeManager upgradeManager = GameManager.Instance.Upgrades;
        foreach (Transform child in upgradeContainer)
        {
            UpgradeUI upgradeUI = child.GetComponent<UpgradeUI>();
            if (upgradeUI != null)
            {
                string upgradeID = upgradeUI.UpgradeID;
                Button buyButton = upgradeUI.GetComponentInChildren<Button>();
                if (buyButton != null)
                {
                    bool canPurchase = upgradeManager.CanPurchaseUpgrade(upgradeID);
                    buyButton.interactable = canPurchase;
                }
            }
        }
    }
    
    /// <summary>
    /// Gets upgrade cost string.
    /// </summary>
    /// <param name="cost"></param>
    /// <returns></returns>
    private string GetUpgradeCostString(Dictionary<string, float> cost)
    {
        string costText = "Cost:";
        foreach (var costItem in cost)
        {
            ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources().Find(r => r.Definition.ID == costItem.Key)?.Definition;
            string resourceName = resource != null ? resource.DisplayName : costItem.Key;
            costText += $"\n{resourceName}: {costItem.Value}";
        }
        return costText;
    }
    
    #endregion
    
    #region Tooltip
    
    /// <summary>
    /// Show the tooltip with specified content.
    /// </summary>
    public void ShowTooltip(string targetID, UIType type, string message = null)
    {
        tooltipPanel.SetActive(true);
        string costText = "Cost:";
        
        switch (type)
        {
            case UIType.Building:
                Building building = GameManager.Instance.Buildings.GetBuilding(targetID);
                if (building != null)
                {
                    var currentCost = GameManager.Instance.Buildings.CalculateBuildingCost(targetID);
                    tooltipTitle.text = building.Definition.DisplayName;
                    tooltipDescription.text = building.Definition.Description;
                    
                    foreach (var costItem in currentCost)
                    {
                        bool canAfford = GameManager.Instance.Resources.GetAmount(costItem.Key) >= costItem.Value;
                        string colorTag = canAfford ? "<color=white>" : "<color=red>";
                        
                        ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                            .Find(r => r.Definition.ID == costItem.Key)?.Definition;
                        string resourceName = resource != null ? resource.DisplayName : costItem.Key;
                        
                        costText += $"\n{colorTag}{resourceName}: {costItem.Value}</color>";
                    }
                    tooltipCost.text = costText;
                    tooltipEffects.text = GetBuildingEffectsString(building.Definition);
                }
                break;
            case UIType.Upgrade:
                Upgrade upgrade = GameManager.Instance.Upgrades.GetUpgrade(targetID);
                if (upgrade != null)
                {
                    tooltipTitle.text = upgrade.Definition.DisplayName;
                    tooltipDescription.text = upgrade.Definition.Description;
                    
                    foreach (var costItem in upgrade.Definition.Cost)
                    {
                        bool canAfford = GameManager.Instance.Resources.GetAmount(costItem.Key) >= costItem.Value;
                        string colorTag = canAfford ? "<color=white>" : "<color=red>";
                        
                        ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                            .Find(r => r.Definition.ID == costItem.Key)?.Definition;
                        string resourceName = resource != null ? resource.DisplayName : costItem.Key;
                        
                        costText += $"\n{colorTag}{resourceName}: {costItem.Value}</color>";
                    }
                    tooltipCost.text = costText;
                    tooltipEffects.text = GetUpgradeEffectsString(upgrade.Definition);
                }
                break;
            default:
                tooltipTitle.text = "Error";
                tooltipDescription.text = "Invalid tooltip type.";
                tooltipCost.text = "";
                tooltipEffects.text = "";
                break;
        }
        
        if (!string.IsNullOrEmpty(message))
        {
            tooltipDescription.text = message;
        }
    }
    
    /// <summary>
    /// Hide the tooltip.
    /// </summary>
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Get building effects.
    /// </summary>
    private string GetBuildingEffectsString(BuildingDefinition building)
    {
        string effectsText = "Effects:";
        foreach (var production in building.Production)
        {
            ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                .Find(r => r.Definition.ID == production.ResourceID)?.Definition;
            string resourceName = resource != null ? resource.DisplayName : production.ResourceID;
            effectsText += $"\n+{production.Amount}/s {resourceName}";
        }
        
        foreach (var consumption in building.Consumption)
        {
            ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                .Find(r => r.Definition.ID == consumption.ResourceID)?.Definition;
            string resourceName = resource != null ? resource.DisplayName : consumption.ResourceID;
            effectsText += $"\n-{consumption.Amount}/s {resourceName}";
        }
        
        foreach (var capacity in building.Capacity)
        {
            ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                .Find(r => r.Definition.ID == capacity.ResourceID)?.Definition;
            string resourceName = resource != null ? resource.DisplayName : capacity.ResourceID;
            effectsText += $"\n+{capacity.Amount} {resourceName} Capacity";
        }
        
        if (effectsText == "Effects:")
        {
            effectsText = "Effects:\nNone";
        }
        return effectsText;
    }
    
    /// <summary>
    /// Get upgrade effects string.
    /// </summary>
    private string GetUpgradeEffectsString(UpgradeDefinition upgrade)
    {
        string effectsText = "Effects:";
        foreach (var effect in upgrade.Effects)
        {
            switch (effect.Type)
            {
                case EffectType.ProductionMultiplier:
                    ResourceDefinition prodResource = GameManager.Instance.Resources.GetVisibleResources()
                        .Find(r => r.Definition.ID == effect.TargetID)?.Definition;
                    string prodResourceName = prodResource != null ? prodResource.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {prodResourceName} production";
                    break;
                case EffectType.BuildingProductionMultiplier:
                    BuildingDefinition buildingDef = GameManager.Instance.Buildings.GetBuildingDefinition(effect.TargetID);
                    string buildingName = buildingDef != null ? buildingDef.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {buildingName} production";
                    break;
                case EffectType.ResourceCapacityMultiplier:
                    ResourceDefinition capResource = GameManager.Instance.Resources.GetVisibleResources()
                       .Find(r => r.Definition.ID == effect.TargetID)?.Definition;
                    string capResourceName = capResource != null ? capResource.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {capResourceName} capacity";
                    break;
                case EffectType.UnlockBuilding:
                    BuildingDefinition unlockBuildingDef = GameManager.Instance.Buildings.GetBuildingDefinition(effect.TargetID);
                    string unlockBuildingName = unlockBuildingDef != null ? unlockBuildingDef.DisplayName: effect.TargetID;
                    effectsText += $"\nUnlocks {unlockBuildingName}";
                    break;
                case EffectType.UnlockUpgrade:
                    UpgradeDefinition unlockUpgradeDef = GameManager.Instance.Upgrades.GetUpgradeDefinition(effect.TargetID);
                    string unlockUpgradeName = unlockUpgradeDef != null ? unlockUpgradeDef.DisplayName : effect.TargetID;
                    effectsText += $"\nUnlocks {unlockUpgradeName}";
                    break;
                default:
                    effectsText += $"\nUnknown effect type: {effect.Type}";
                    break;
            }
        }
        if (effectsText == "Effects:")
        {
            effectsText = "Effects:\nNone";
        }
        return effectsText;
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Clear all children from a transform.
    /// </summary>
    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
    
    #endregion
}

/// <summary>
/// Enum for UI tabs
/// </summary>
public enum UITab
{
    Buildings,
    Upgrades
}

/// <summary>
/// Enum for UI element types for tooltips
/// </summary>
public enum UIType
{
    Building,
    Upgrade
}
