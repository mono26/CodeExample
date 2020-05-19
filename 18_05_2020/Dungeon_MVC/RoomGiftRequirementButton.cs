using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Bound.Dungeon
{
    public class RoomGiftRequirementButton : BoundElement
    {
        public event Action<int> OnGiftButtonPressed;

        [SerializeField]
        private Image giftFable = null;
        [SerializeField]
        private Image giftFrame = null;
        [SerializeField]
        private Image giftImage = null;
        [SerializeField]
        private TextMeshProUGUI givenAndRequired;

        private string giftId = null;
        private int amountGiven = 0;
        private int requiredAmount = 0;
        private int page = -1;

        private void Start()
        {
            RoomGift.OnGiftsGiven += OnGiftsGiven;
        }

        private void OnDestroy()
        {
            RoomGift.OnGiftsGiven -= OnGiftsGiven;
        }

        public void InitializeButton(RoomGiftRequirement _gift, int _page)
        {
            giftId = _gift.giftId;
            page = _page;

            requiredAmount = _gift.amount;

            Room tempReference = DungeonController.GetCurrentRoom;
            int level = DungeonController.GetRoomLevel(tempReference.RoomNumber);
            RoomUnlockableProgress unlockableProgress = DungeonController.GetRoomUnlockableProgress(tempReference.RoomNumber, level);
            amountGiven = unlockableProgress.GetAmountGiven(_gift.giftId);

            DungeonGift catalogReference = PFWrapper.instance.GetCachedCatalogEntry<DungeonGift>(_gift.giftId);

            RoomGiftRequirementView.DisplayGiftImage(catalogReference, giftImage);
            RoomGiftRequirementView.DisplayGiftFable(catalogReference, giftFable);
            RoomGiftRequirementView.DisplayGiftQuality(catalogReference, giftFrame);

            RoomGiftRequirementView.DisplayGivenAndRequired(amountGiven, requiredAmount, givenAndRequired);
        }

        private void OnGiftsGiven(string _id, int _amountGiven)
        {
            if (giftId.Equals(_id))
            {
                amountGiven += _amountGiven;
                RoomGiftRequirementView.DisplayGivenAndRequired(amountGiven, requiredAmount, givenAndRequired);
            }
        }

        public void OnButtonPress()
        {
            OnGiftButtonPressed?.Invoke(page);
        }
    }
}
