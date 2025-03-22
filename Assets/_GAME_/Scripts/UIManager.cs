using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            resourceUI.ValueText.text = capacity > 0 
                ? $"{Mathf.Floor(amount)}/{Mathf.Floor(capacity)}" 
                : $"{Mathf.Floor(amount)}";
            
            // Production rate
            float productionRate = CalculateNetProductionRate(resourceID);
            string rateText = productionRate >= 0 
                ? $"+{productionRate:F1}/s" 
                : $"{productionRate:F1}/s";
            
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
        // We're just showing an approximation for the player
        
        float productionRate = 0;
        float consumptionRate = 0;
        
        // Get production from buildings
        var buildings = GameManager.Instance.Buildings.GetAllBuildings();
        foreach (var building in buildings)
        {
            if (building.Count <= 0) continue;
            
            // Production
            foreach (var production in building.Definition.Production)
            {
                if (production.ResourceID == resourceID)
                {
                    productionRate += production.Amount * building.Count;
                }
            }
            
            // Consumption
            foreach (var consumption in building.Definition.Consumption)
            {
                if (consumption.ResourceID == resourceID)
                {
                    consumptionRate += consumption.Amount * building.Count;
                }
            }
        }
        
        // Apply multipliers from upgrades
        float multiplier = 1f;
        var upgrades = GameManager.Instance.Upgrades.GetAllPurchasedUpgrades();
        foreach (var upgrade in upgrades)
        {
            foreach (var effect in upgrade.Definition.Effects)
            {
                if (effect.Type == EffectType.ProductionMultiplier && effect.TargetID == resourceID)
                {
                    multiplier *= effect.Value;
                }
                else if (effect.Type == EffectType.ConsumptionReduction && effect.TargetID == resourceID)
                {
                    consumptionRate *= (1f - effect.Value); // Reduce consumption
                }
            }
        }
        
        return (productionRate * multiplier) - consumptionRate;
    }
    
    /// <summary>
    /// Refresh the entire resource view (called when visibility changes)
    /// </summary>
    public void RefreshResourceView()
    {
        // Get visible resources
        List<Resource> visibleResources = GameManager.Instance.Resources.GetVisibleResources();
        
        // Remove resources that are no longer visible
        List<string> resourcesToRemove = new List<string>();
        foreach (var resourceID in resourceUIElements.Keys)
        {
            bool stillVisible = false;
            foreach (var resource in visibleResources)
            {
                if (resource.Definition.ID == resourceID)
                {
                    stillVisible = true;
                    break;
                }
            }
            
            if (!stillVisible)
            {
                resourcesToRemove.Add(resourceID);
            }
        }
        
        foreach (var resourceID in resourcesToRemove)
        {
            Destroy(resourceUIElements[resourceID].GameObject);
            resourceUIElements.Remove(resourceID);
        }
        
        // Add newly visible resources
        foreach (var resource in visibleResources)
        {
            if (!resourceUIElements.ContainsKey(resource.Definition.ID))
            {
                CreateResourceUI(resource);
            }
        }
        
        // Update values
        UpdateResourceValues();
    }
    
    /// <summary>
    /// Create UI element for a resource
    /// </summary>
    private void CreateResourceUI(Resource resource)
    {
        GameObject resourceUI = Instantiate(resourcePrefab, resourceContainer);
        
        // Get components
        TMP_Text nameText = resourceUI.transform.Find("NameText").GetComponent<TMP_Text>();
        TMP_Text valueText = resourceUI.transform.Find("ValueText").GetComponent<TMP_Text>();
        TMP_Text rateText = resourceUI.transform.Find("RateText").GetComponent<TMP_Text>();
        Image resourceIcon = resourceUI.transform.Find("Icon").GetComponent<Image>();
        
        // Set up UI
        nameText.text = resource.Definition.DisplayName;
        resourceIcon.sprite = resource.Definition.Icon;
        
        // Add to tracking
        resourceUIElements[resource.Definition.ID] = new ResourceUI
        {
            GameObject = resourceUI,
            ResourceID = resource.Definition.ID,
            NameText = nameText,
            ValueText = valueText,
            RateText = rateText,
            Icon = resourceIcon
        };
    }
    
    #endregion
    
    #region Building UI
    
    /// <summary>
    /// Refresh the entire building view (called when visibility changes)
    /// </summary>
    public void RefreshBuildingView()
    {
        // Clear existing building buttons
        ClearContainer(buildingContainer);
        
        // Get visible buildings
        List<Building> visibleBuildings = GameManager.Instance.Buildings.GetVisibleBuildings();
        
        // Create UI for each visible building
        foreach (var building in visibleBuildings)
        {
            CreateBuildingUI(building);
        }
    }
    
    /// <summary>
    /// Create UI element for a building
    /// </summary>
    private void CreateBuildingUI(Building building)
    {
        GameObject buildingUI = Instantiate(buildingPrefab, buildingContainer);
        
        // Get components
        TMP_Text nameText = buildingUI.transform.Find("NameText").GetComponent<TMP_Text>();
        TMP_Text countText = buildingUI.transform.Find("CountText").GetComponent<TMP_Text>();
        Button buildButton = buildingUI.GetComponent<Button>();
        Image buildingIcon = buildingUI.transform.Find("Icon").GetComponent<Image>();
        
        // Set up UI
        nameText.text = building.Definition.DisplayName;
        countText.text = building.Count.ToString();
        
        if (building.Definition.Icon != null)
        {
            buildingIcon.sprite = building.Definition.Icon;
        }
        
        // Add button functionality
        string buildingID = building.Definition.ID;
        buildButton.onClick.AddListener(() => OnBuildingClicked(buildingID));
        
        // Set up tooltip
        TooltipTrigger trigger = buildingUI.AddComponent<TooltipTrigger>();
        trigger.TooltipType = TooltipType.Building;
        trigger.ID = buildingID;
        trigger.UIManager = this;
        
        // Update affordability
        UpdateBuildingButtonState(buildingUI, buildingID);
    }
    
    /// <summary>
    /// Update all building buttons (for affordability)
    /// </summary>
    private void UpdateBuildingButtons()
    {
        foreach (Transform child in buildingContainer)
        {
            if (child.gameObject.activeSelf)
            {
                TooltipTrigger trigger = child.GetComponent<TooltipTrigger>();
                if (trigger != null && trigger.TooltipType == TooltipType.Building)
                {
                    UpdateBuildingButtonState(child.gameObject, trigger.ID);
                }
            }
        }
    }
    
    /// <summary>
    /// Update a building button's state based on affordability
    /// </summary>
    private void UpdateBuildingButtonState(GameObject buttonObj, string buildingID)
    {
        Button button = buttonObj.GetComponent<Button>();
        
        bool canAfford = GameManager.Instance.Buildings.CanConstructBuilding(buildingID);
        button.interactable = canAfford;
        
        // Update count text
        TMP_Text countText = buttonObj.transform.Find("CountText").GetComponent<TMP_Text>();
        Building building = GameManager.Instance.Buildings.GetBuilding(buildingID);
        if (building != null)
        {
            countText.text = building.Count.ToString();
        }
    }
    
    /// <summary>
    /// Handle click on a building button
    /// </summary>
    private void OnBuildingClicked(string buildingID)
    {
        GameManager.Instance.Buildings.ConstructBuilding(buildingID);
    }
    
    #endregion
    
    #region Upgrade UI
    
    /// <summary>
    /// Refresh the entire upgrade view (called when visibility changes)
    /// </summary>
    public void RefreshUpgradeView()
    {
        // Clear existing upgrade buttons
        ClearContainer(upgradeContainer);
        
        // Get visible upgrades
        List<Upgrade> visibleUpgrades = GameManager.Instance.Upgrades.GetVisibleUpgrades();
        
        // Create UI for each visible upgrade
        foreach (var upgrade in visibleUpgrades)
        {
            CreateUpgradeUI(upgrade);
        }
    }
    
    /// <summary>
    /// Create UI element for an upgrade
    /// </summary>
    private void CreateUpgradeUI(Upgrade upgrade)
    {
        GameObject upgradeUI = Instantiate(upgradePrefab, upgradeContainer);
        
        // Get components
        TMP_Text nameText = upgradeUI.transform.Find("NameText").GetComponent<TMP_Text>();
        Button purchaseButton = upgradeUI.GetComponent<Button>();
        Image upgradeIcon = upgradeUI.transform.Find("Icon").GetComponent<Image>();
        
        // Set up UI
        nameText.text = upgrade.Definition.DisplayName;
        
        if (upgrade.Definition.Icon != null)
        {
            upgradeIcon.sprite = upgrade.Definition.Icon;
        }
        
        // Add button functionality
        string upgradeID = upgrade.Definition.ID;
        purchaseButton.onClick.AddListener(() => OnUpgradeClicked(upgradeID));
        
        // Update affordability
        bool canAfford = GameManager.Instance.Upgrades.CanPurchaseUpgrade(upgradeID);
        purchaseButton.interactable = canAfford;
        
        // Set up tooltip
        TooltipTrigger trigger = upgradeUI.AddComponent<TooltipTrigger>();
        trigger.TooltipType = TooltipType.Upgrade;
        trigger.ID = upgradeID;
        trigger.UIManager = this;
    }
    
    /// <summary>
    /// Handle click on an upgrade button
    /// </summary>
    private void OnUpgradeClicked(string upgradeID)
    {
        GameManager.Instance.Upgrades.PurchaseUpgrade(upgradeID);
    }
    
    #endregion
    
    #region Tooltip System
    
    /// <summary>
    /// Show tooltip for a building
    /// </summary>
    public void ShowBuildingTooltip(string buildingID)
    {
        Building building = GameManager.Instance.Buildings.GetBuilding(buildingID);
        if (building == null) return;
        
        // Set tooltip content
        tooltipTitle.text = building.Definition.DisplayName;
        tooltipDescription.text = building.Definition.Description;
        
        // Cost
        Dictionary<string, float> cost = GameManager.Instance.Buildings.CalculateBuildingCost(buildingID);
        string costText = "Cost:";
        foreach (var costItem in cost)
        {
            bool canAfford = GameManager.Instance.Resources.GetAmount(costItem.Key) >= costItem.Value;
            string colorTag = canAfford ? "<color=white>" : "<color=red>";
            
            ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                .Find(r => r.Definition.ID == costItem.Key)?.Definition;
            string resourceName = resource != null ? resource.DisplayName : costItem.Key;
            
            costText += $"\n{colorTag}{resourceName}: {Mathf.Ceil(costItem.Value)}</color>";
        }
        tooltipCost.text = costText;
        
        // Effects (production, consumption, storage)
        string effectsText = "";
        
        // Production
        if (building.Definition.Production.Count > 0)
        {
            effectsText += "Production:";
            foreach (var production in building.Definition.Production)
            {
                ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                    .Find(r => r.Definition.ID == production.ResourceID)?.Definition;
                string resourceName = resource != null ? resource.DisplayName : production.ResourceID;
                
                effectsText += $"\n+{production.Amount:F1} {resourceName}/s";
            }
        }
        
        // Consumption
        if (building.Definition.Consumption.Count > 0)
        {
            if (effectsText.Length > 0) effectsText += "\n\n";
            effectsText += "Consumption:";
            foreach (var consumption in building.Definition.Consumption)
            {
                ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                    .Find(r => r.Definition.ID == consumption.ResourceID)?.Definition;
                string resourceName = resource != null ? resource.DisplayName : consumption.ResourceID;
                
                effectsText += $"\n-{consumption.Amount:F1} {resourceName}/s";
            }
        }
        
        // Storage
        if (building.Definition.Capacity.Count > 0)
        {
            if (effectsText.Length > 0) effectsText += "\n\n";
            effectsText += "Storage:";
            foreach (var capacity in building.Definition.Capacity)
            {
                ResourceDefinition resource = GameManager.Instance.Resources.GetVisibleResources()
                    .Find(r => r.Definition.ID == capacity.ResourceID)?.Definition;
                string resourceName = resource != null ? resource.DisplayName : capacity.ResourceID;
                
                effectsText += $"\n+{capacity.Amount:F0} {resourceName}";
            }
        }
        
        tooltipEffects.text = effectsText;
        
        // Show tooltip
        tooltipPanel.SetActive(true);
    }
    
    /// <summary>
    /// Show tooltip for an upgrade
    /// </summary>
    public void ShowUpgradeTooltip(string upgradeID)
    {
        Upgrade upgrade = GameManager.Instance.Upgrades.GetUpgrade(upgradeID);
        if (upgrade == null) return;
        
        // Set tooltip content
        tooltipTitle.text = upgrade.Definition.DisplayName;
        tooltipDescription.text = upgrade.Definition.Description;
        
        // Cost
        string costText = "Cost:";
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
        
        // Effects
        string effectsText = "Effects:";
        foreach (var effect in upgrade.Definition.Effects)
        {
            switch (effect.Type)
            {
                case EffectType.ProductionMultiplier:
                    ResourceDefinition prodResource = GameManager.Instance.Resources.GetVisibleResources()
                        .Find(r => r.Definition.ID == effect.TargetID)?.Definition;
                    string prodResourceName = prodResource != null ? prodResource.DisplayName : effect.TargetID;
                    effectsText += $"\n+{(effect.Value - 1) * 100:F0}% {prodResourceName} production";
                    break;