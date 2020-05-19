using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Timba.Bound.Dungeon
{
    public class ItemsPanel : DungeonSubpanel
    {
        [SerializeField]
        private UnityEngine.UI.Button upgradeAll;
        [SerializeField]
        private RectTransform itemsContainer = null;
        [SerializeField]
        private RoomItemRequirementButton itemButtonPrefab = null;
        [SerializeField]
        private RoomItemRequirementView requiredItemView = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI upgradeAllCostText = null;

        //private List<RoomItem> requiredItems = new List<RoomItem>();
        private string[] requiredItemsIds;

        #region Properties
        /// <summary>
        /// Gets the item view prefab object.
        /// </summary>
        private RoomItemRequirementButton ItemButtonPrefab
        {
            get
            {
                if (!itemButtonPrefab)
                {
                    itemButtonPrefab = App.Resources.GetAssetFromResources<RoomItemRequirementButton>(nameof(RoomItemRequirementButton));
                }

                return itemButtonPrefab;
            }
        }
        #endregion

        private void Start()
        {
            upgradeAll.onClick.AddListener(OnUpgradeAllButtonPressed);
        }

        private void OnDestroy()
        {
            upgradeAll.onClick.AddListener(OnUpgradeAllButtonPressed);
        }

        /// <summary>
        /// Initialized the items panel.
        /// </summary>
        /// <param name="_currentRoom">Reference to the current active room.</param>
        /// <param name="_parentView">Reference to the room view.</param>
        public void InitializePanel(Room _currentRoom)
        {
            requiredItemView.InitilizeView(DungeonController.GetRoomProgress(_currentRoom.RoomNumber).ItemsProgress);

            LoadRoomRequiredItems(_currentRoom.RequiredItems);

            DisplayRequiredItems(_currentRoom.RoomNumber);
        }

        /// <summary>
        /// Loads the room required items.
        /// </summary>
        /// <param name="_room">Room to load items from.</param>
        private void LoadRoomRequiredItems(RoomItemRequirement[] _requiredItems)
        {
            requiredItemsIds = new string[_requiredItems.Length];
            for (int i = 0; i < _requiredItems.Length; i++)
            {
                requiredItemsIds[i] = _requiredItems[i].ItemId;
            }
        }

        #region View
        /// <summary>
        /// Displays all the required items for the room.
        /// </summary>
        /// <param name="_roomNumber">Number of the room.</param>
        private void DisplayRequiredItems(int _roomNumber)
        {
            // TODO use same objects without destroyin. Maybe use objects from scene directly.
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < requiredItemsIds.Length; i++)
            {
                RoomItem item = DungeonController.GetRoomItemProgress(_roomNumber, requiredItemsIds[i]);
                DisplayRequiredItemButton(item, i);
            }
        }

        /// <summary>
        /// Displays a single item view.
        /// </summary>
        /// <param name="_itemToDisplay">Id of the item to display.</param>
        private void DisplayRequiredItemButton(RoomItem _itemToDisplay, int _page)
        {
            RoomItemRequirementButton itemButton = Instantiate(ItemButtonPrefab, itemsContainer);

            itemButton.InitializeButton(_itemToDisplay, _page);

            itemButton.OnItemButtonPressed += OnItemButtonPressed;
        }

        private void OnItemButtonPressed(int _page)
        {
            requiredItemView.OpenPage(_page);
        }

        /// <summary>
        /// Displays the cost for upgrading all items.
        /// </summary>
        private void DisplayUpgradeAllCost() => upgradeAllCostText.text = DungeonController.GetUpgradeAllCost(DungeonController.GetCurrentRoom.RoomNumber, requiredItemsIds).ToString();
        #endregion

        #region Conditionals
        /// <summary>
        /// Checks if all the required items can be upgraded to the next level.
        /// </summary>
        /// <returns></returns>
        private bool CanUpgradeAllItems()
        {
            bool canUpgrade = true;
            foreach (string item in requiredItemsIds)
            {
                if (!CanUpgradeItem(item))
                {
                    canUpgrade = false;
                    break;
                }
            }

            return canUpgrade;
        }

        public bool CanUpgradeItem(string _itemId)
        {
            return DungeonController.CanUpgradeItem(DungeonController.GetCurrentRoom.RoomNumber, _itemId);
        }
        #endregion

        #region Event listeners
        /// <summary>
        /// Called when the upgrade all items button is pressed.
        /// </summary>
        private void OnUpgradeAllButtonPressed()
        {
            DungeonController.RequestMultipleItemsUpgrade(DungeonController.GetCurrentRoom.RoomNumber, requiredItemsIds);
        }
        #endregion

        public void HideItemsPanel()
        {
            Close();
            requiredItemView.Close(); 
        }

        #region Cheats
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void UpgradeItemsCheat()
        {
            List<RoomItem> itemsToUpgrade = new List<RoomItem>();
            foreach (string item in requiredItemsIds)
            {
                if (!CanUpgradeItem(item))
                {
                    GenericPopup.Show("Affection test pending!", $"There's a pending affection test for { item } level, unlock it before continuing.");
                }
                // If there is no unlockable pending or the unlockable is already unlocked we can upgrade.
                else
                {
                    itemsToUpgrade.Add(DungeonController.GetRoomItemProgress(DungeonController.GetCurrentRoom.RoomNumber, item));
                }
            }

            for (int i = 0; i < itemsToUpgrade.Count; i++)
            {
                itemsToUpgrade[i].UpgradeItem();
            }

            GameStateManager.instance.StartWait("Upgrading items.");

            DungeonController.SaveDungeonProgress(null, null);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void GrantItemsToInventoryCheat()
        {
            App.Repository.GrantMultipleDungeonItems(requiredItemsIds, () =>
            {
                // Super hacky
                InitializePanel(DungeonController.GetCurrentRoom);
            }, null);
        }
        #endregion
    }
}