using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System;

namespace Timba.Bound.Dungeon
{
    public class RoomView : UI.UIElement
    {
        public static event Action<string> OnRoomOpened;
        public static event Action OnGiftsShow = null;
        public static event Action OnItemsShow = null;

        [Header("Subpanels")]
        [SerializeField] private DungeonSubpanel internalHeader = null;
        [SerializeField, FormerlySerializedAs("itemsPanel")] public ItemsPanel itemsPanel = null;
        [SerializeField, FormerlySerializedAs("giftsPanel")] public GiftsPanel giftsPanel = null;
        [SerializeField, FormerlySerializedAs("readyToShow")] private DungeonSubpanel readyToShow = null;
        [SerializeField, FormerlySerializedAs("thatWasFun")] private DungeonSubpanel thatWasFun = null;

        [SerializeField]
        private Image princess = null;
        [SerializeField]
        private RoomLevelScroll levelScroll = null;
        [SerializeField]
        private TextMeshProUGUI roomTitle = null;
        [SerializeField]
        private TextMeshProUGUI roomLevel = null;

        private GenericPopupBound genericPopupBound = null;
        private Room currentRoom;

        private void Awake()
        {
            genericPopupBound = GetComponent<GenericPopupBound>();
        }

        private void Start()
        {
            DungeonController.RoomDisplay = this;

            DungeonController.OnRoomSelected += Display;

            RoomItem.OnUpgrade += OnItemUpgrade;
            RoomItem.OnUnlock += OnItemUpgrade;

            DungeonController.OnUnlockableEnded += OnUnlockableEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DungeonController.RoomDisplay = null;

            DungeonController.OnRoomSelected -= Display;

            RoomItem.OnUpgrade -= OnItemUpgrade;
            RoomItem.OnUnlock -= OnItemUpgrade;

            DungeonController.OnUnlockableEnded -= OnUnlockableEnded;
        }

        /// <summary>
        /// Displays a room.
        /// </summary>
        /// <param name="_room">Room to display.</param>
        private void Display(Room _room)
        {
            genericPopupBound.LoadState();

            currentRoom = _room;

            DisplayRoomPrincess(currentRoom);

            DisplayRoomName();

            StartCoroutine(internalHeader.SetState(true));

            OnOpenUIElement();

            OnRoomOpened?.Invoke($"room_{_room.RoomNumber}");
        }

        public void OnRoomIn()
        {
            levelScroll.Initialize(currentRoom.RoomNumber);

            DisplayGiftsOrItemsPanel();

            DisplayRoomLevel();
        }

        /// <summary>
        /// Displays the room princes.
        /// </summary>
        /// <param name="_room">Room to display princess for.</param>
        private void DisplayRoomPrincess(Room _room)
        {
            Sprite princessSprite = App.Resources.GetAssetFromResources<Sprite>(string.Format("DialogueHeroes/{0}", _room.PrincessId));
            if (princessSprite != null)
            {
                princess.sprite = princessSprite;
            }
            else
            {
                princess.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Displays the room name.
        /// </summary>
        private void DisplayRoomName()
        {
            roomTitle.text = LocalizationHelper.GetLocalizedTerm(string.Format("Name/tid_{0}", currentRoom.PrincessId));
        }

        public void CloseRoom()
        {
            genericPopupBound.CloseState();
            OnCloseUIElement();
        }
       
        /// <summary>
        /// Levels up the room according to the items level.
        /// </summary>
        public void UpdateView()
        {
            DisplayRoomLevel();

            DisplayGiftsOrItemsPanel();
        }

        private void DisplayGiftsOrItemsPanel()
        {
            bool showGifts = DungeonController.IsThereAUnlockablePending(
                currentRoom.RoomNumber,
                DungeonController.GetRoomLevel(currentRoom.RoomNumber)
                );
            ToggleStatePanel(showGifts);
        }

        private void DisplayRoomLevel()
        {
            int level = DungeonController.GetRoomLevel(currentRoom.RoomNumber);
            SetScrollPage();
            roomLevel.text = $"Room lvl { level }";
        }

        public void SetScrollPage() 
        {
            levelScroll.SetRoomLevel(DungeonController.GetRoomLevel(currentRoom.RoomNumber));
        }

        /// <summary>
        /// Checks if there is a pending unlockable and displays the affection test.
        /// </summary>
        private void ToggleStatePanel(bool _gifts)
        {
            // TODO don't do this if the panel is already active.
            if (_gifts)
            {
                itemsPanel.HideItemsPanel();
                giftsPanel.InitializePanel(currentRoom);

                int level = DungeonController.GetRoomLevel(currentRoom.RoomNumber);
                RoomUnlockableProgress unlockableProgress = DungeonController.GetRoomUnlockableProgress(currentRoom.RoomNumber, level);
                // TODO check if there are any pending gifts.
                bool allGiven = true;
                foreach (var gift in unlockableProgress.GiftsProgress)
                {
                    if (gift.AmountGiven < gift.RequiredAmount)
                    {
                        allGiven = false;
                        break;
                    }
                }

                if (allGiven)
                {
                    GameStateManager.instance.StartWait("Giving gifts to princess.");

                    unlockableProgress.UnlockUnlockable();

                    DungeonController.SaveDungeonProgress(() =>
                    {
                        RoomUnlockableData unlockableToUnlock = DungeonController.GetUnlockableData(currentRoom.RoomNumber, level);
                        DungeonController.ShowAfectionReward(unlockableToUnlock);
                    },
                    () =>
                    {
                        Debugging.DebugController.LogErrorMessage("An unexpected error ocurred while saving unlockable progress.");
                    });

                    return;
                }

                StartCoroutine(DisplayGiftsPanelCoroutine());
            }
            else
            {
                giftsPanel.HideGiftsPanel();
                itemsPanel.InitializePanel(currentRoom);
                StartCoroutine(itemsPanel.SetState(true));
                OnItemsShow?.Invoke();
            }
        }

        private IEnumerator DisplayGiftsPanelCoroutine()
        {
            StartCoroutine(readyToShow.SetState(true));
            yield return new WaitUntil(() => readyToShow.gameObject.activeInHierarchy == false);
            StartCoroutine(giftsPanel.SetState(true));
            OnGiftsShow?.Invoke();
        }

        public void SetUI(bool _value)
        {
            if (!_value)
            {
                itemsPanel.HideItemsPanel();
                internalHeader.Close();
            }
            else 
            {
                StartCoroutine(itemsPanel.SetState(true));
                StartCoroutine(internalHeader.SetState(true));
            }
        }

        public override void CloseElement()
        {
            CloseRoom();
        }

        private void OnItemUpgrade(RoomItem _item)
        {
            DisplayRoomLevel();

            // TODO put data storage inside model.
            int currentRoomNumber = currentRoom.RoomNumber;
            if (DungeonController.IsThereAUnlockablePending(currentRoomNumber, DungeonController.GetRoomLevel(currentRoomNumber)))
            {
                DisplayGiftsOrItemsPanel();
            }
        }

        private void OnUnlockableEnded()
        {
            DisplayRoomLevel();
            DisplayGiftsOrItemsPanel();
        }
    }
}
