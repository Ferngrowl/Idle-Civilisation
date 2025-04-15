using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Linq;
using Game.Interfaces;
using Game.Models;
using GameConfiguration;

/// <summary>
/// Manages all UI elements and interactions in the game. Handles resources display, building lists,
/// upgrade panels, and tooltips.
/// </summary>
public class UIManager : MonoBehaviour, IUIManager
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

    // Manager references through interfaces
    private IResourceManager resourceManager;
    private IBuildingManager buildingManager;
    private IUpgradeManager upgradeManager;
    private ITimeManager timeManager;
    
    private void Awake()
    {
        // Register with ServiceLocator
        ServiceLocator.Register<IUIManager>(this);
    }
    
    private void Start()
    {
        // Get manager references through ServiceLocator
        resourceManager = ServiceLocator.Get<IResourceManager>();
        buildingManager = ServiceLocator.Get<IBuildingManager>();
        upgradeManager = ServiceLocator.Get<IUpgradeManager>();
        timeManager = ServiceLocator.Get<ITimeManager>();
    }
    
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
        resourceUpdateTimer += UnityEngine.Time.deltaTime;
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
        float productionRate = resourceManager.GetProductionRate(resourceID);
        float consumptionRate = resourceManager.GetConsumptionRate(resourceID);
        return productionRate - consumptionRate;
    }
    
    /// <summary>
    /// Refresh the entire resource view. Called on initialization and when resources change visibility.
    /// </summary>
    public void RefreshResourceView()
    {
        List<Game.Models.Resource> visibleResources = resourceManager.GetVisibleResources();
        
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
                    ConvertCostDictionaryToList(buildingManager.CalculateBuildingCost(building.Definition.ID)),
                    building.Definition.Icon
                );
                
                // Set up the button click handler
                GameObject buildingObj = buildingUI.gameObject;
                Button buyButton = buildingObj.GetComponentInChildren<Button>();
                if (buyButton != null)
                {
                    buyButton.onClick.AddListener(() =>
                    {
                        if (buildingManager.CanConstructBuilding(building.Definition.ID))
                        {
                            buildingManager.ConstructBuilding(building.Definition.ID);
                            UpdateBuildingCount(building.Definition.ID); //update count
                            UpdateResourceValues(); //update resources
                            UpdateBuildingButtons();
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
            Building building = buildingManager.GetBuilding(buildingID);
            buildingUI.CountText.text = $"Owned: {building.Count}";
            
            // Calculate current cost and convert to List<ResourceAmount>
            Dictionary<string, float> currentCostDict = 
                buildingManager.CalculateBuildingCost(buildingID);
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
            bool canAfford = buildingManager.CanConstructBuilding(entry.Key);
            
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
                        if (upgradeManager.CanPurchaseUpgrade(upgrade.Definition.ID))
                        {
                            upgradeManager.PurchaseUpgrade(upgrade.Definition.ID);
                            UpdateUpgradeButtons(); 
                            UpdateResourceValues();
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
        BuildingDefinition buildingDef = buildingManager.GetBuildingDefinition(buildingID);
        if (buildingDef == null) return;
        
        // Get building count
        int currentCount = buildingManager.GetBuildingCount(buildingID);
        
        // Set basic information
        if (tooltipTitle != null)
            tooltipTitle.text = buildingDef.DisplayName;
        
        if (tooltipDescription != null)
            tooltipDescription.text = buildingDef.Description + $"\nOwned: {currentCount}";
        
        // Format costs with affordability colors
        if (tooltipCost != null)
        {
            string costText = "Cost:";
            Dictionary<string, float> costs = buildingManager.CalculateBuildingCost(buildingID);
            
            foreach (var cost in costs)
            {
                ResourceDefinition resource = resourceManager.GetResourceDefinition(cost.Key);
                if (resource == null) continue;
                
                bool canAfford = resourceManager.GetAmount(cost.Key) >= cost.Value;
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
                    ResourceDefinition resource = resourceManager.GetResourceDefinition(prod.ResourceID);
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
                    ResourceDefinition resource = resourceManager.GetResourceDefinition(cons.ResourceID);
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
                    ResourceDefinition resource = resourceManager.GetResourceDefinition(storage.ResourceID);
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
        UpgradeDefinition upgradeDef = upgradeManager.GetUpgradeDefinition(upgradeID);
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
                ResourceDefinition resource = resourceManager.GetResourceDefinition(costItem.ResourceID);
                if (resource == null) continue;
                
                bool canAfford = resourceManager.GetAmount(costItem.ResourceID) >= costItem.Amount;
                string colorTag = canAfford ? "<color=green>" : "<color=red>";
                
                costText += $"\n{colorTag}{resource.DisplayName}: {costItem.Amount}</color>";
            }
            
            tooltipCost.text = costText;
        }
        
        // Format effects using DataFormatter
        if (tooltipEffects != null)
        {
            tooltipEffects.text = DataFormatter.GetEffectsString(ConvertUpgradeEffects(upgradeDef.Effects));
        }
    }
    
    /// <summary>
    /// Shows resource tooltip with description
    /// </summary>
    private void ShowResourceTooltip(string resourceID)
    {
        // Get resource data
        ResourceDefinition resource = resourceManager.GetResourceDefinition(resourceID);
        if (resource == null) return;
        
        // Set basic information only
        if (tooltipTitle != null)
            tooltipTitle.text = resource.DisplayName;
        
        if (tooltipDescription != null)
        {
            float amount = resourceManager.GetAmount(resourceID);
            float capacity = resourceManager.GetCapacity(resourceID);
            float productionRate = CalculateNetProductionRate(resourceID);
            
            string description = resource.Description;
            
            if (capacity > 0)
            {
                description += $"\n\nAmount: {amount:F1}/{capacity:F1}";
            }
            else
            {
                description += $"\n\nAmount: {amount:F1}";
            }
            
            if (productionRate != 0)
            {
                string ratePrefix = productionRate > 0 ? "+" : "";
                description += $"\nRate: {ratePrefix}{productionRate:F2}/s";
            }
            
            tooltipDescription.text = description;
        }
        
        // Clear cost and effects for resource tooltips
        if (tooltipCost != null)
            tooltipCost.text = "";
        
        if (tooltipEffects != null)
            tooltipEffects.text = "";
    }

    /// <summary>
    /// Show tooltip at a specific UI element
    /// </summary>
    public void ShowTooltip(string content, RectTransform target)
    {
        // Prevent rapid flickering
        if (Time.unscaledTime - lastTooltipStateChange < TOOLTIP_DEBOUNCE_TIME && isTooltipShowing)
            return;
            
        lastTooltipStateChange = Time.unscaledTime;
        isTooltipShowing = true;
        
        // Clear previous content and activate
        tooltipTitle.text = "";
        tooltipCost.text = "";
        tooltipEffects.text = "";
        tooltipDescription.text = content;
        tooltipPanel.SetActive(true);
        
        // Format tooltip and position it
        FormatTooltip();
        PositionTooltipAtTarget(target);
    }
    
    /// <summary>
    /// Format the tooltip contents (adjust sizing, etc.)
    /// </summary>
    private void FormatTooltip()
    {
        // Force the layout system to update immediately
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)tooltipPanel.transform);
        
        // Hide any empty sections
        bool hasCost = !string.IsNullOrEmpty(tooltipCost.text);
        bool hasEffects = !string.IsNullOrEmpty(tooltipEffects.text);
        
        // Get parents
        Transform costParent = tooltipCost.transform.parent;
        Transform effectsParent = tooltipEffects.transform.parent;
        
        if (costParent != null) costParent.gameObject.SetActive(hasCost);
        if (effectsParent != null) effectsParent.gameObject.SetActive(hasEffects);
        
        // Force another layout update after hiding/showing sections
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)tooltipPanel.transform);
    }
    
    /// <summary>
    /// Position tooltip next to mouse cursor
    /// </summary>
    private void PositionTooltipAtMouse()
    {
        if (tooltipPanel == null) return;
        
        Vector2 position = Input.mousePosition;
        
        // Get tooltip size
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        
        // Check screen edges
        float rightEdge = position.x + tooltipSize.x + cursorOffset.x + edgePadding;
        float bottomEdge = position.y + tooltipSize.y + cursorOffset.y + edgePadding;
        float leftEdge = position.x + cursorOffset.x - edgePadding;
        float topEdge = position.y + cursorOffset.y - edgePadding;
        
        // Adjust position if tooltip would go off-screen
        if (rightEdge > Screen.width)
        {
            position.x = position.x - tooltipSize.x - Mathf.Abs(cursorOffset.x);
        }
        else
        {
            position.x += cursorOffset.x;
        }
        
        if (topEdge < 0)
        {
            position.y = position.y + tooltipSize.y + Mathf.Abs(cursorOffset.y);
        }
        else if (bottomEdge > Screen.height)
        {
            position.y = position.y - tooltipSize.y - Mathf.Abs(cursorOffset.y);
        }
        else
        {
            position.y += cursorOffset.y;
        }
        
        // Update tooltip position
        tooltipRect.position = position;
    }
    
    /// <summary>
    /// Position tooltip relative to a target UI element
    /// </summary>
    private void PositionTooltipAtTarget(RectTransform target)
    {
        if (tooltipPanel == null || target == null) return;
        
        // Calculate position
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        
        // Find center of target
        Vector2 position = (corners[0] + corners[2]) / 2;
        
        // Get tooltip size
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        
        // Position to the right if there's space
        if (position.x + tooltipSize.x + edgePadding < Screen.width)
        {
            position.x = corners[2].x + edgePadding;
        }
        else
        {
            // Otherwise, position to the left
            position.x = corners[0].x - tooltipSize.x - edgePadding;
        }
        
        // Center vertically if possible
        float targetHeight = corners[2].y - corners[0].y;
        position.y = corners[0].y + (targetHeight - tooltipSize.y) / 2;
        
        // Adjust for screen edges
        if (position.y + tooltipSize.y > Screen.height)
        {
            position.y = Screen.height - tooltipSize.y - edgePadding;
        }
        else if (position.y < 0)
        {
            position.y = edgePadding;
        }
        
        // Update tooltip position
        tooltipRect.position = position;
    }
    
    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideTooltip(bool trackStateChange = true)
    {
        if (trackStateChange)
        {
            lastTooltipStateChange = Time.unscaledTime;
        }
        
        isTooltipShowing = false;
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Empty a UI container
    /// </summary>
    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        
        // Destroy all children
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// Convert a cost dictionary to a list of ResourceAmount
    /// </summary>
    private List<ResourceAmount> ConvertCostDictionaryToList(Dictionary<string, float> costDict)
    {
        List<ResourceAmount> costList = new List<ResourceAmount>();
        foreach (var kvp in costDict)
        {
            costList.Add(new ResourceAmount
            {
                ResourceID = kvp.Key,
                Amount = kvp.Value
            });
        }
        return costList;
    }
    
    /// <summary>
    /// Convert GameConfiguration.UpgradeEffect to UpgradeEffect
    /// </summary>
    private List<UpgradeEffect> ConvertUpgradeEffects(List<GameConfiguration.UpgradeEffect> effects)
    {
        List<UpgradeEffect> convertedEffects = new List<UpgradeEffect>();
        
        foreach (var effect in effects)
        {
            convertedEffects.Add(new UpgradeEffect
            {
                Type = (EffectType)(int)effect.Type,
                TargetID = effect.TargetID,
                Value = effect.Value
            });
        }
        
        return convertedEffects;
    }
    
    #endregion
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
