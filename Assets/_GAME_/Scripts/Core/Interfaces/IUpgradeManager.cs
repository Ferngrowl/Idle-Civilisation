using System.Collections.Generic;
using Game.Models;
using GameConfiguration;
using Serialization;

namespace Game.Interfaces
{
    /// <summary>
    /// Interface for the UpgradeManager component
    /// Manages upgrades and their effects on the game
    /// </summary>
    public interface IUpgradeManager
    {
        // Upgrade access
        List<Upgrade> GetAllUpgrades();
        List<Upgrade> GetAllPurchasedUpgrades();
        List<Upgrade> GetVisibleUpgrades();
        UpgradeDefinition GetUpgradeDefinition(string upgradeID);
        
        // Upgrade operations
        bool CanPurchaseUpgrade(string upgradeID);
        bool PurchaseUpgrade(string upgradeID);
        bool HasUpgrade(string upgradeID);
        
        // Lifecycle methods
        void Initialize();
        void Tick(long tickNumber);
        void Reset();
        
        // Save/load
        Serialization.UpgradeSaveData SerializeData();
        void DeserializeData(Serialization.UpgradeSaveData data);
    }
} 