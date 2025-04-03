using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Linq;

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
    private Dictionary<string, BuildingUI> buildingUIElements = new Dictionary<string, BuildingUI>();
    
    [Header("Upgrade UI")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private TMP_Text upgradeTabButton;
    [SerializeField] private GameObject upgradePanel;
    private Dictionary<string, UpgradeUI> upgradeUIElements = new Dictionary<string, UpgradeUI>(); 
    
    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private TMP_Text tooltipCost;
    [SerializeField] private TMP_Text tooltipEffects;

    [Header("Tab Visuals")]
    [SerializeField] private GameObject buildingTabHighlight;
    [SerializeField] private GameObject upgradeTabHighlight;
    
    // UI State tracking
    private UITab currentTab = UITab.Buildings;
    
    // Tooltip state
    private bool isTooltipShowing = false;
    private float lastTooltipStateChange = 0f;
    private const float TOOLTIP_DEBOUNCE_TIME = 0.1f;
    [Header("Tooltip Settings")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(15, -15);
    [SerializeField] private float edgePadding = 10f;
    
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
            
            // Also update building and upgrade buttons (for affordability)
            UpdateBuildingButtons();
            UpdateUpgradeButtons();
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

    public void OnClickButtonPressed()
    {
        GameManager.Instance.Resources.AddResource("wood", 1);
    }
    
    /// <summary>
    /// Update tab button visual states
    /// </summary>
    private void UpdateTabButtons()
    {
        buildingTabHighlight.SetActive(currentTab == UITab.Buildings);
        upgradeTabHighlight.SetActive(currentTab == UITab.Upgrades);
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
            float productionRate = CalculateNetProductionRate(resourceID);
            
            // Use the improved ResourceUI methods
            resourceUI.UpdateResourceValue(amount, capacity, productionRate);
            
            // Calculate time until full/empty
            if (capacity > 0 && productionRate != 0)
            {
                bool isFilling = productionRate > 0;
                float timeRemaining;
                
                if (isFilling)
                {
                    timeRemaining = (capacity - amount) / productionRate;
                    resourceUI.UpdateTimeDisplay(timeRemaining, true);
                }
                else
                {
                    timeRemaining = amount / -productionRate;
                    resourceUI.UpdateTimeDisplay(timeRemaining, false);
                }
            }
            else
            {
                resourceUI.UpdateTimeDisplay(-1, false);
            }
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

        // Force layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(resourceContainer.GetComponent<RectTransform>());
        
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
        buildingUIElements.Clear();
        
        // Create UI elements for visible buildings
        foreach (var building in visibleBuildings)
        {
            GameObject buildingUIObj = Instantiate(buildingPrefab, buildingContainer);
            BuildingUI buildingUI = buildingUIObj.GetComponent<BuildingUI>();
            buildingUIElements[building.Definition.ID] = buildingUI;

            if (buildingUI != null)
            {
                // Initialize the BuildingUI with building data
                buildingUI.Initialize(
                    building.Definition.ID,
                    building.Definition.DisplayName,
                    building.Definition.Description,
                    building.Count,
                    ConvertCostDictionaryToList(GameManager.Instance.Buildings.CalculateBuildingCost(building.Definition.ID)),
                    building.Definition.Icon
                );
                
                // Set up the button click handler
                GameObject buildingObj = buildingUI.gameObject;
                Button buyButton = buildingObj.GetComponentInChildren<Button>();
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
                        }
                        else
                        {
                            // Optionally, show a message in the tooltip
                            // ShowTooltip(building.Definition.ID, UIType.Building, "Can't afford!"); -- Removed
                        }
                    });
                }
                else
                {
                    Debug.LogError($"Building prefab is missing Button component: {building.Definition.ID}");
                }
            }
            else
            {
                Debug.LogError($"Building prefab is missing BuildingUI component: {building.Definition.ID}");
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(buildingContainer.GetComponent<RectTransform>());

        UpdateBuildingButtons();
    }
    
    /// <summary>
    /// Update the display of a building's count.
    /// </summary>
    private void UpdateBuildingCount(string buildingID)
    {
        if (buildingUIElements.TryGetValue(buildingID, out BuildingUI buildingUI))
        {
            Building building = GameManager.Instance.Buildings.GetBuilding(buildingID);
            buildingUI.CountText.text = $"Owned: {building.Count}";
            
            // Calculate current cost and convert to List<ResourceAmount>
            Dictionary<string, float> currentCostDict = 
                GameManager.Instance.Buildings.CalculateBuildingCost(buildingID);
            List<ResourceAmount> currentCostList = currentCostDict
                .Select(kvp => new ResourceAmount { ResourceID = kvp.Key, Amount = kvp.Value })
                .ToList();
            
            buildingUI.CostText.text = GetBuildingCostString(currentCostList);
        }
    }
    
    /// <summary>
    /// Update building buttons, specifically their interactability based on resource availability.
    /// </summary>
    private void UpdateBuildingButtons()
    {
        foreach (var entry in buildingUIElements)
        {
            BuildingUI buildingUI = entry.Value;
            bool canAfford = GameManager.Instance.Buildings.CanConstructBuilding(entry.Key);
            
            // Update both container and button
            GameObject buildingObj = buildingUI.gameObject;
            Button buyButton = buildingObj.GetComponentInChildren<Button>();
            if (buyButton != null)
            {
                buyButton.interactable = canAfford;
                
                // Change text color for better visibility
                TMP_Text buttonText = buyButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.color = canAfford ? Color.white : new Color(0.3f, 0.3f, 0.3f); // Dark gray for better contrast
                }
            }
            
        }
    }
    
    /// <summary>
    /// Get the cost string.
    /// </summary>
    private string GetBuildingCostString(List<ResourceAmount> cost)
    {
        return DataFormatter.GetCostString(cost);
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
        upgradeUIElements.Clear();
        
        // Create UI elements for visible upgrades
        foreach (var upgrade in visibleUpgrades)
        {
            GameObject upgradeUIObj = Instantiate(upgradePrefab, upgradeContainer);
            UpgradeUI upgradeUI = upgradeUIObj.GetComponent<UpgradeUI>();
            upgradeUIElements.Add(upgrade.Definition.ID, upgradeUI);
            
            if (upgradeUI != null)
            {
                // Initialize the UpgradeUI with upgrade data
                upgradeUI.Initialize(
                    upgrade.Definition.ID,
                    upgrade.Definition.DisplayName,
                    upgrade.Definition.Description,
                    upgrade.Definition.Cost,
                    upgrade.Definition.Icon
                );
                
                // Set up the button click handler
                GameObject upgradeObj = upgradeUI.gameObject;
                Button buyButton = upgradeObj.GetComponentInChildren<Button>();
                if (buyButton != null)
                {
                    buyButton.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance.Upgrades.CanPurchaseUpgrade(upgrade.Definition.ID))
                        {
                            GameManager.Instance.Upgrades.PurchaseUpgrade(upgrade.Definition.ID);
                            UpdateUpgradeButtons(); 
                            UpdateResourceValues();
                        }
                        else
                        {
                            // ShowTooltip(upgrade.Definition.ID, UIType.Upgrade, "Can't afford!"); -- Removed
                        }
                    });
                }
                else
                {
                    Debug.LogError($"Upgrade prefab is missing Button component: {upgrade.Definition.ID}");
                }
            }
            else
            {
                Debug.LogError($"Upgrade prefab is missing UpgradeUI component: {upgrade.Definition.ID}");
            }
        }

        // Force layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(upgradeContainer.GetComponent<RectTransform>());
        UpdateUpgradeButtons();
    }
    
    /// <summary>
    /// Updates the Upgrade buttons.
    /// </summary>
    private void UpdateUpgradeButtons()
    {
        UpgradeManager upgradeManager = GameManager.Instance.Upgrades;
        foreach (var upgradeUI in upgradeUIElements.Values)
        {
            string upgradeID = upgradeUI.UpgradeID;
            GameObject upgradeObj = upgradeUI.gameObject;
            Button buyButton = upgradeObj.GetComponentInChildren<Button>();
            if (buyButton != null)
            {
                bool canPurchase = upgradeManager.CanPurchaseUpgrade(upgradeID);
                buyButton.interactable = canPurchase;
                TMP_Text buttonText = buyButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.color = canPurchase ? Color.white : new Color(0.3f, 0.3f, 0.3f);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets upgrade cost string.
    /// </summary>
    /// <param name="cost"></param>
    /// <returns></returns>
    private string GetUpgradeCostString(List<ResourceAmount> cost)
    {
        return DataFormatter.GetCostString(cost);
    }
    
    #endregion
    
    #region Tooltip Management
    
    /// <summary>
    /// Shows tooltip with detailed information based on target ID and type
    /// </summary>
    public void ShowTooltip(string targetID, UIType uiType, string message = null)
    {
        // Prevent rapid flickering by debouncing
        if (Time.unscaledTime - lastTooltipStateChange < TOOLTIP_DEBOUNCE_TIME && isTooltipShowing)
            return;
        
        lastTooltipStateChange = Time.unscaledTime;
        
        // Clear previous content and activate tooltip
        HideTooltip(false);
        
        if (tooltipPanel != null)
            tooltipPanel.SetActive(true);
        
        isTooltipShowing = true;
        
        // Set content based on type
        switch (uiType)
        {
            case UIType.Building:
                ShowBuildingTooltip(targetID);
                break;
            case UIType.Upgrade:
                ShowUpgradeTooltip(targetID);
                break;
            case UIType.Resource:
                ShowResourceTooltip(targetID);
                break;
            default:
                if (!string.IsNullOrEmpty(message) && tooltipDescription != null)
                    tooltipDescription.text = message;
                break;
        }
        
        // Format and position tooltip
        FormatTooltip();
        PositionTooltipAtMouse();
    }
    
    /// <summary>
    /// Shows building tooltip with costs and effects
    /// </summary>
    private void ShowBuildingTooltip(string buildingID)
    {
        // Get building data
        BuildingDefinition buildingDef = GameManager.Instance.Buildings.GetBuildingDefinition(buildingID);
        if (buildingDef == null) return;
        
        // Get building count
        int currentCount = GameManager.Instance.Buildings.GetBuildingCount(buildingID);
        
        // Set basic information
        if (tooltipTitle != null)
            tooltipTitle.text = buildingDef.DisplayName;
        
        if (tooltipDescription != null)
            tooltipDescription.text = buildingDef.Description + $"\nOwned: {currentCount}";
        
        // Format costs with affordability colors
        if (tooltipCost != null)
        {
            string costText = "Cost:";
            Dictionary<string, float> costs = GameManager.Instance.Buildings.CalculateBuildingCost(buildingID);
            
            foreach (var cost in costs)
            {
                ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(cost.Key);
                if (resource == null) continue;
                
                bool canAfford = GameManager.Instance.Resources.GetAmount(cost.Key) >= cost.Value;
                string colorTag = canAfford ? "<color=green>" : "<color=red>";
                
                costText += $"\n{colorTag}{resource.DisplayName}: {cost.Value}</color>";
            }
            
            tooltipCost.text = costText;
        }
        
        // Format effects (production, consumption, storage)
        if (tooltipEffects != null)
        {
            string effectsText = "Effects:";
            
            // Production
            if (buildingDef.Production.Count > 0)
            {
                effectsText += "\nProduction:";
                foreach (var prod in buildingDef.Production)
                {
                    ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(prod.ResourceID);
                    if (resource != null)
                        effectsText += $"\n+{prod.Amount} {resource.DisplayName}";
                }
            }
            
            // Consumption
            if (buildingDef.Consumption.Count > 0)
            {
                effectsText += "\nConsumption:";
                foreach (var cons in buildingDef.Consumption)
                {
                    ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(cons.ResourceID);
                    if (resource != null)
                        effectsText += $"\n-{cons.Amount} {resource.DisplayName}";
                }
            }
            
            // Storage
            if (buildingDef.Capacity.Count > 0)
            {
                effectsText += "\nStorage:";
                foreach (var storage in buildingDef.Capacity)
                {
                    ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(storage.ResourceID);
                    if (resource != null)
                        effectsText += $"\n+{storage.Amount} {resource.DisplayName}";
                }
            }
            
            tooltipEffects.text = effectsText;
        }
    }
    
    /// <summary>
    /// Shows upgrade tooltip with costs and effects
    /// </summary>
    private void ShowUpgradeTooltip(string upgradeID)
    {
        // Get upgrade data
        UpgradeDefinition upgradeDef = GameManager.Instance.Upgrades.GetUpgradeDefinition(upgradeID);
        if (upgradeDef == null) return;
        
        // Set basic information
        if (tooltipTitle != null)
            tooltipTitle.text = upgradeDef.DisplayName;
        
        if (tooltipDescription != null)
            tooltipDescription.text = upgradeDef.Description;
        
        // Format costs with affordability colors
        if (tooltipCost != null)
        {
            string costText = "Cost:";
            
            foreach (var costItem in upgradeDef.Cost)
            {
                ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(costItem.ResourceID);
                if (resource == null) continue;
                
                bool canAfford = GameManager.Instance.Resources.GetAmount(costItem.ResourceID) >= costItem.Amount;
                string colorTag = canAfford ? "<color=green>" : "<color=red>";
                
                costText += $"\n{colorTag}{resource.DisplayName}: {costItem.Amount}</color>";
            }
            
            tooltipCost.text = costText;
        }
        
        // Format effects using DataFormatter
        if (tooltipEffects != null)
        {
            tooltipEffects.text = DataFormatter.GetEffectsString(upgradeDef.Effects);
        }
    }
    
    /// <summary>
    /// Shows resource tooltip with description
    /// </summary>
    private void ShowResourceTooltip(string resourceID)
    {
        // Get resource data
        ResourceDefinition resource = GameManager.Instance.Resources.GetResourceDefinition(resourceID);
        if (resource == null) return;
        
        // Set basic information only
        if (tooltipTitle != null)
            tooltipTitle.text = resource.DisplayName;
        
        if (tooltipDescription != null)
            tooltipDescription.text = resource.Description;
        
        // Clear other sections
        if (tooltipCost != null)
            tooltipCost.text = string.Empty;
        
        if (tooltipEffects != null)
            tooltipEffects.text = string.Empty;
    }
    
    /// <summary>
    /// Formats the tooltip for optimal display
    /// </summary>
    private void FormatTooltip()
    {
        // Enable word wrapping for all text components
        if (tooltipDescription != null && tooltipDescription is TMP_Text tmpDescription)
            tmpDescription.enableWordWrapping = true;
        
        if (tooltipEffects != null && tooltipEffects is TMP_Text tmpEffects)
            tmpEffects.enableWordWrapping = true;
        
        if (tooltipCost != null && tooltipCost is TMP_Text tmpCost)
            tmpCost.enableWordWrapping = true;
        
        // Ensure tooltip has minimum size and background
        if (tooltipPanel != null)
        {
            // Set minimum size
            LayoutElement layout = tooltipPanel.GetComponent<LayoutElement>();
            if (layout == null)
                layout = tooltipPanel.AddComponent<LayoutElement>();
            
            layout.minWidth = 200;
            layout.minHeight = 100;
            
            // Make sure there's a dark background
            Image background = tooltipPanel.GetComponent<Image>();
            if (background == null)
                background = tooltipPanel.AddComponent<Image>();
            
            // Set dark grey semi-transparent background for better text visibility
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Force layout rebuild to get correct dimensions
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());
        }
    }
    
    /// <summary>
    /// Positions tooltip near mouse cursor with screen edge protection
    /// </summary>
    private void PositionTooltipAtMouse()
    {
        if (tooltipPanel == null || !tooltipPanel.gameObject.activeSelf)
            return;
        
        // Get tooltip dimensions
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (tooltipRect == null) return;
        
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 mousePosition = Input.mousePosition;
        
        // Add small buffer to prevent flickering due to exact edge calculations
        const float edgeBuffer = 2f;
        
        // Calculate optimal position (offset from cursor)
        Vector2 position = mousePosition + cursorOffset;
        
        // Adjust for screen edges
        if (position.x + tooltipSize.x > Screen.width - edgePadding - edgeBuffer)
            position.x = mousePosition.x - tooltipSize.x - cursorOffset.x;
        
        if (position.y + tooltipSize.y > Screen.height - edgePadding - edgeBuffer)
            position.y = mousePosition.y - tooltipSize.y - cursorOffset.y;
        
        if (position.x < edgePadding + edgeBuffer)
            position.x = edgePadding + edgeBuffer;
        
        if (position.y < edgePadding + edgeBuffer)
            position.y = edgePadding + edgeBuffer;
        
        // Apply position
        tooltipRect.position = position;
    }
    
    /// <summary>
    /// Hides and clears tooltip content
    /// </summary>
    public void HideTooltip(bool deactivatePanel = true)
    {
        // Prevent rapid flickering by debouncing (unless explicitly clearing content)
        if (deactivatePanel && Time.unscaledTime - lastTooltipStateChange < TOOLTIP_DEBOUNCE_TIME && !isTooltipShowing)
            return;
        
        if (deactivatePanel)
            lastTooltipStateChange = Time.unscaledTime;
        
        isTooltipShowing = false;
        
        // Clear text to prevent flicker
        if (tooltipTitle != null)
            tooltipTitle.text = string.Empty;
        
        if (tooltipDescription != null)
            tooltipDescription.text = string.Empty;
        
        if (tooltipCost != null)
            tooltipCost.text = string.Empty;
        
        if (tooltipEffects != null)
            tooltipEffects.text = string.Empty;
        
        // Hide panel if requested
        if (deactivatePanel && tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Returns whether tooltip is currently active
    /// </summary>
    public bool IsTooltipActive()
    {
        return isTooltipShowing;
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
    
    /// <summary>
    /// Convert a Dictionary<string, float> to List<ResourceAmount>
    /// </summary>
    private List<ResourceAmount> ConvertCostDictionaryToList(Dictionary<string, float> costDictionary)
    {
        List<ResourceAmount> result = new List<ResourceAmount>();
        
        foreach (var kvp in costDictionary)
        {
            result.Add(new ResourceAmount
            {
                ResourceID = kvp.Key,
                Amount = kvp.Value
            });
        }
        
        return result;
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
    Upgrade,
    Resource
}
