using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles tooltip display for UI elements using pointer events
/// </summary>
[RequireComponent(typeof(Image))]
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Configuration")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private string targetID;
    [SerializeField] private UIType tooltipType;
    
    [Header("Visual Settings")]
    [Tooltip("Show hover area boundaries in the editor")]
    [SerializeField] private bool showDebugOutline = false;
    
    // Timing settings to prevent flickering
    private const float HOVER_DELAY = 0.1f;
    private const float TRANSITION_DELAY = 0.05f;
    
    // State tracking
    private bool isHovering = false;
    private Coroutine activeCoroutine = null;
    private static TooltipTrigger activeTooltip = null;
    private static bool isTransitioning = false;
    
    // Global static timer to prevent rapid toggling
    private static float lastTooltipTime = 0f;
    private static readonly float DEBOUNCE_TIME = 0.05f;
    
    #region Unity Lifecycle
    
    private void Start()
    {
        // Get necessary references
        if (uiManager == null) 
            uiManager = FindObjectOfType<UIManager>();
            
        // Set up debug visualization if enabled
        if (showDebugOutline)
            SetupDebugVisuals();
    }
    
    private void OnDisable()
    {
        if (isHovering)
        {
            CancelActiveCoroutine();
            HideTooltip();
        }
    }
    
    private void OnDestroy()
    {
        if (activeTooltip == this)
        {
            CancelActiveCoroutine();
            HideTooltip();
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Skip if already handled or we're in a transitional state
        if (eventData.used || isTransitioning || isHovering || IsPointerOverButton(eventData))
            return;
        
        // Debounce rapid enter/exit events
        if (Time.unscaledTime - lastTooltipTime < DEBOUNCE_TIME)
            return;
        
        lastTooltipTime = Time.unscaledTime;
        isHovering = true;
        CancelActiveCoroutine();
        activeCoroutine = StartCoroutine(ShowTooltipDelayed());
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.used || isTransitioning || !isHovering)
            return;
        
        // Get the object currently under pointer before handling exit
        TooltipTrigger nextTooltip = GetTooltipUnderPointer();
        
        // Only debounce if we're not moving to another tooltip
        if (nextTooltip == null && Time.unscaledTime - lastTooltipTime < DEBOUNCE_TIME)
            return;
        
        lastTooltipTime = Time.unscaledTime;
        
        // Special handling if we're moving to another tooltip
        if (nextTooltip != null && nextTooltip != this)
        {
            // Transition directly to the next tooltip
            isTransitioning = true;
            StartCoroutine(ClearTransitionFlag());
            
            isHovering = false;
            CancelActiveCoroutine();
            HideTooltip();
            return;
        }
        
        // Normal exit behavior with slightly longer delay to prevent flickering
        isHovering = false;
        CancelActiveCoroutine();
        activeCoroutine = StartCoroutine(HideTooltipDelayed());
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Show tooltip for this object
    /// </summary>
    public void ShowTooltip()
    {
        if (!isHovering)
        {
            isHovering = true;
            ShowTooltipInternal();
        }
    }
    
    /// <summary>
    /// Hide tooltip for this object
    /// </summary>
    public void HideTooltip()
    {
        isHovering = false;
        CancelActiveCoroutine();
        HideTooltipInternal();
    }
    
    #endregion
    
    #region Internal Implementation
    
    private void ShowTooltipInternal()
    {
        // Validate required data
        if (string.IsNullOrEmpty(targetID) || uiManager == null)
            return;
        
        // Hide any existing tooltip from a different trigger
        if (activeTooltip != null && activeTooltip != this)
        {
            activeTooltip.HideTooltipInternal();
        }
        
        // Set as active and display
        activeTooltip = this;
        uiManager.ShowTooltip(targetID, tooltipType);
    }
    
    private void HideTooltipInternal()
    {
        if (activeTooltip == this && uiManager != null)
        {
            uiManager.HideTooltip();
            activeTooltip = null;
        }
    }
    
    private IEnumerator ShowTooltipDelayed()
    {
        // Use slightly longer delay to ensure stability
        yield return new WaitForSecondsRealtime(HOVER_DELAY);
        
        // Double-check we're still hovering (prevents flickering)
        if (isHovering && activeTooltip != this)
            ShowTooltipInternal();
        
        activeCoroutine = null;
    }
    
    private IEnumerator HideTooltipDelayed()
    {
        // Use slightly longer delay for hiding to prevent flickering
        yield return new WaitForSecondsRealtime(TRANSITION_DELAY * 2);
        
        // Only hide if we're still not hovering
        if (!isHovering)
            HideTooltipInternal();
        
        activeCoroutine = null;
    }
    
    // Use unscaled time for transition flag to ensure consistency regardless of time scale
    private IEnumerator ClearTransitionFlag()
    {
        yield return new WaitForSecondsRealtime(TRANSITION_DELAY * 2);
        isTransitioning = false;
    }
    
    private void CancelActiveCoroutine()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
    }
    
    private void SetupDebugVisuals()
    {
        Outline outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(2, 2);
    }
    
    private bool IsPointerOverButton(PointerEventData eventData)
    {
        return eventData.pointerCurrentRaycast.gameObject != null && 
               eventData.pointerCurrentRaycast.gameObject.GetComponent<Button>() != null;
    }
    
    private TooltipTrigger GetTooltipUnderPointer()
    {
        GameObject hoveredObj = GetObjectUnderPointer();
        return hoveredObj?.GetComponent<TooltipTrigger>();
    }
    
    private GameObject GetObjectUnderPointer()
    {
        if (EventSystem.current == null)
            return null;
            
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        return results.Count > 0 ? results[0].gameObject : null;
    }
    
    #endregion
    
    #region Static Creator API
    
    /// <summary>
    /// Adds tooltip functionality to a game object
    /// </summary>
    /// <param name="targetObject">UI element that will trigger the tooltip</param>
    /// <param name="uiManager">Reference to the UI manager</param>
    /// <param name="targetID">ID of the item (building, upgrade)</param>
    /// <param name="type">Type of tooltip to display</param>
    /// <returns>The created tooltip component</returns>
    public static TooltipTrigger AddTooltip(GameObject targetObject, UIManager uiManager, string targetID, UIType type)
    {
        if (targetObject == null || uiManager == null || string.IsNullOrEmpty(targetID))
        {
            Debug.LogWarning("Cannot add tooltip: missing required parameters");
            return null;
        }
        
        // Remove any existing tooltip triggers
        TooltipTrigger existingTrigger = targetObject.GetComponent<TooltipTrigger>();
        if (existingTrigger != null)
            DestroyImmediate(existingTrigger);
        
        // Prepare object for tooltip events
        EnsureRaycastTarget(targetObject);
        DisableTextRaycasting(targetObject);
        
        // Create trigger component
        TooltipTrigger trigger = targetObject.AddComponent<TooltipTrigger>();
        trigger.uiManager = uiManager;
        trigger.targetID = targetID;
        trigger.tooltipType = type;
        
        return trigger;
    }
    
    private static void EnsureRaycastTarget(GameObject targetObject)
    {
        // Get existing Image or add one
        Image image = targetObject.GetComponent<Image>();
        if (image == null)
        {
            // Add a completely transparent image that only serves as a raycast target
            image = targetObject.AddComponent<Image>();
        }
        
        // Make image completely invisible but still raycast detectable
        // This creates an invisible rectangle that detects mouse hovering
        Color transparent = new Color(0, 0, 0, 0);
        image.color = transparent;
        
        // Ensure raycast detection is enabled
        image.raycastTarget = true;
    }
    
    private static void DisableTextRaycasting(GameObject targetObject)
    {
        // Disable raycasting on text elements to prevent interference
        foreach (Text text in targetObject.GetComponentsInChildren<Text>())
            text.raycastTarget = false;
        
        foreach (TMP_Text tmpText in targetObject.GetComponentsInChildren<TMP_Text>())
            tmpText.raycastTarget = false;
    }
    
    #endregion
}