using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Bound.Dungeon
{
    public class GiftsPanel : DungeonSubpanel
    {
        [SerializeField]
        private Button giveAllGiftsButton = null;
        [SerializeField]
        private RectTransform giftsContainer = null;
        [SerializeField]
        private RoomGiftRequirementButton requiredGiftButtonPrefab = null;
        [SerializeField]
        private RoomGiftRequirementView requiredGiftView = null;

        // TODO change this to ids.
        private List<RoomGiftRequirement> currentGiftsRequired = new List<RoomGiftRequirement>();

        private RoomGiftRequirementButton GiftButtonPrefab
        {
            get
            {
                if (!requiredGiftButtonPrefab)
                {
                    requiredGiftButtonPrefab = App.Resources.GetAssetFromResources<RoomGiftRequirementButton>(nameof(RoomGiftRequirementButton));
                }

                return requiredGiftButtonPrefab;
            }
        }

        private void Start()
        {
            RoomUnlockableProgress.OnRoomUnlockableUnlocked += OnUnlockableUnlock;
        }

        private void OnDestroy()
        {
            RoomUnlockableProgress.OnRoomUnlockableUnlocked -= OnUnlockableUnlock;
        }

        private void OnUnlockableUnlock(int _unlockableLevel)
        {
            HideGiftsPanel();
        }

        /// <summary>
        /// Initializes the gifts panel.
        /// </summary>
        /// <param name="_currentRoom">Reference to the current active room.</param>
        /// <param name="_parentView">Reference to the room view.</param>
        public void InitializePanel(Room _currentRoom)
        {
            int level = DungeonController.GetRoomLevel(_currentRoom.RoomNumber);
            RoomUnlockableData unlockable = DungeonController.GetUnlockableData(_currentRoom.RoomNumber, level);

            requiredGiftView.InitializeView(unlockable.unlockRequirement);
            DisplayRequiredGifts(unlockable.unlockRequirement);
        }

        public void HideGiftsPanel()
        {
            Close();
            requiredGiftView.Close();
        }

        #region Display
        /// <summary>
        /// Displays the required gifts for a single unlockable.
        /// </summary>
        /// <param name="_gifts">Unlockable to display requirements.</param>
        public void DisplayRequiredGifts(RoomGiftRequirement[] _gifts)
        {
            foreach (Transform child in giftsContainer)
            {
                Destroy(child.gameObject);
            }

            currentGiftsRequired.Clear();

            for (int i = 0; i < _gifts.Length; i++)
            {
                currentGiftsRequired.Add(_gifts[i]);
                DisplayRequiredGiftButton(_gifts[i], i);
            }
        }

        /// <summary>
        /// Display a single gift button.
        /// </summary>
        /// <param name="_giftToDisplay">Git to display.</param>
        private void DisplayRequiredGiftButton(RoomGiftRequirement _giftToDisplay, int _page)
        {
            RoomGiftRequirementButton giftButton = Instantiate(GiftButtonPrefab, giftsContainer);
            giftButton.InitializeButton(_giftToDisplay, _page);

            giftButton.OnGiftButtonPressed += OnGiftButtonPressed;
        }

        private void OnGiftButtonPressed(int _page)
        {
            requiredGiftView.GoToPage(_page);
        }


        #endregion

        #region Request
        /// <summary>
        /// Called when the gifts panel request to give the gifts to the current room princes.
        /// </summary>
        public void OnGiveGiftsButtonPressed()
        {
            DungeonController.UnlockDungeonUnlockable(DungeonController.GetCurrentRoom.RoomNumber, DungeonController.GetRoomLevel(DungeonController.GetCurrentRoom.RoomNumber));
        }
        #endregion

        #region Cheats
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void GiveGiftsCheat()
        {
            int roomNumber = DungeonController.GetCurrentRoom.RoomNumber;
            int roomLevel = DungeonController.GetRoomLevel(roomNumber);
            if (DungeonController.IsThereAUnlockablePending(roomNumber, roomLevel))
            {
                GameStateManager.instance.StartWait("Giving gifts to princess.");

                RoomUnlockableProgress unlockableProgress = DungeonController.GetRoomUnlockableProgress(roomNumber, roomLevel);
                unlockableProgress.UnlockUnlockable();

                DungeonController.SaveDungeonProgress(() =>
                {
                    RoomUnlockableData unlockableToUnlock = DungeonController.GetUnlockableData(roomNumber, roomLevel);
                    DungeonController.ShowAfectionReward(unlockableToUnlock);
                },
                () =>
                {
                    GenericPopup.Show("Error", "An unexpected error ocurred while saving unlockable progress.");
                });
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void GrantGiftsToInventoryCheat()
        {
            int roomNumber = DungeonController.GetCurrentRoom.RoomNumber;
            if (DungeonController.IsThereAUnlockablePending(roomNumber, DungeonController.GetRoomLevel(roomNumber)))
            {
                List<string> giftsToGrant = new List<string>();
                foreach (RoomGiftRequirement requiredGift in currentGiftsRequired)
                {
                    for (int i = 0; i < requiredGift.amount; i++)
                    {
                        giftsToGrant.Add(requiredGift.giftId);
                    }
                }
                App.Repository.GrantMultipleDungeonItems(giftsToGrant.ToArray(), () =>
                {
                    InitializePanel(DungeonController.GetCurrentRoom);
                }, null);
            }
        }
        #endregion

        void OnOpeningEnd() { }
    }
}
