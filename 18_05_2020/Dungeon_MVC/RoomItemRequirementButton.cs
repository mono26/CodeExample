using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Bound.Dungeon
{
    public class RoomItemRequirementButton : BoundElement
    {
        public event Action<int> OnItemButtonPressed;

        [SerializeField]
        private Color lockedColor;
        [SerializeField]
        private Image itemIcon = null;
        [SerializeField]
        private GameObject canUpgradeIcon = null;
        [SerializeField]
        private TextMeshProUGUI statusText = null;

        private int page = -1;
        private string itemId = null;
        private string currentIconLvl = null;

        private void Start()
        {
            RoomItem.OnUpgrade += OnItemUpgrade;
            RoomItem.OnUnlock += OnItemUnlock;
        }

        private void OnDestroy()
        {
            RoomItem.OnUpgrade -= OnItemUpgrade;
            RoomItem.OnUnlock -= OnItemUnlock;
        }

        public void InitializeButton(RoomItem _item, int _page)
        {
            itemId = _item.ItemId;
            page = _page;

            DisplayItemImage(_item);

            if (!_item.IsUnlocked)
            {
                canUpgradeIcon.SetActive(false);

                DisplayAvailableItems(_item);
            }
            else
            {
                canUpgradeIcon.SetActive(CanUpgrade());

                DisplayItemLevel(_item);
            }
        }

        private bool CanUpgrade()
        {
            return DungeonController.CanUpgradeItem(DungeonController.GetCurrentRoom.RoomNumber, itemId);
        }

        private void DisplayItemImage(RoomItem _item)
        {
            DisplayItemUnlockedStateColor(_item);

            DisplayItemIcon(_item);
        }

        private void DisplayItemUnlockedStateColor(RoomItem _item)
        {
            Color colorState = _item.IsUnlocked ? Color.white : lockedColor;
            itemIcon.color = colorState;
            itemIcon.SetAllDirty();
        }

        private void DisplayItemIcon(RoomItem _item)
        {
            currentIconLvl = RoomItemRequirementView.GetIconLvl(_item.ItemLvl);
            itemIcon.sprite = App.Resources.GetAssetFromBundles<Sprite>($"DungeonItems/{ _item.ItemId }_{ currentIconLvl }");
        }

        private void DisplayAvailableItems(RoomItem _item)
        {
            var inventory = PFWrapper.instance.GetPlayerInventory();

            int availableAmount = 0;
            if (inventory.Exists(x => x.ItemId == _item.ItemId))
            {
                availableAmount = (int)inventory.Find(x => x.ItemId.Equals(_item.ItemId)).RemainingUses;
            }

            statusText.text = string.Format("{0}/1", availableAmount);
        }

        private void DisplayItemLevel(RoomItem _item)
        {
            statusText.text = string.Format("lvl {0}", _item.ItemLvl);
        }

        public void OnButtonPress()
        {
            OnItemButtonPressed?.Invoke(page);
        }

        private void OnItemUpgrade(RoomItem _upgradedItem)
        {
            if (itemId.Equals(_upgradedItem.ItemId))
            {
                DisplayItemLevel(_upgradedItem);

                // DisplayItemUpgradeStateColor(_upgradedItem);

                if (RoomItemRequirementView.ShouldUpdateItemIcon(_upgradedItem.ItemLvl, currentIconLvl))
                {
                    DisplayItemIcon(_upgradedItem);
                }

                canUpgradeIcon.SetActive(CanUpgrade());
            }
        }

        private void DisplayItemUpgradeStateColor(RoomItem _item) => itemIcon.color = DungeonController.CanUpgradeItem(DungeonController.GetCurrentRoom.RoomNumber, itemId) ? Color.white : lockedColor;

        private void OnItemUnlock(RoomItem _unlockedItem)
        {
            if (itemId.Equals(_unlockedItem.ItemId))
            {
                DisplayItemImage(_unlockedItem);

                DisplayItemLevel(_unlockedItem);

                //DisplayItemUpgradeStateColor(_unlockedItem);

                canUpgradeIcon.SetActive(CanUpgrade());
            }
        }
    }
}
