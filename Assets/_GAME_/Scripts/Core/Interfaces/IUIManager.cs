using UnityEngine;

namespace Game.Interfaces
{
    /// <summary>
    /// Interface for the UIManager component
    /// Manages UI elements and their interactions
    /// </summary>
    public interface IUIManager
    {
        // UI initialization and updates
        void Initialize();
        
        // UI refreshing
        void RefreshResourceView();
        void RefreshBuildingView();
        void RefreshUpgradeView();
        
        // UI element control
        void ShowTooltip(string content, RectTransform target);
        void ShowTooltip(string targetID, UIType type, string message = null);
        void HideTooltip(bool trackStateChange = true);
    }
} 