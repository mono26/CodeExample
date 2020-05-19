using Dialogue;
using DialogueData;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Timba.Bound.Exceptions.Dungeon;
using Timba.Bound.QuestsBound;
using UnityEngine;
using UnityEngine.Events;

namespace Timba.Bound.Dungeon
{
    public class DungeonController : Singleton<DungeonController>
    {
        public static event Action OnUnlockableEnded;
        public static event Action<Room> OnRoomSelected;

        [SerializeField] 
        private Transform animatedSceneCanvas;

        public static int UpgradeCostPerLevel { get => GameDataBoundManager.GetDungeonData.UpgradeCostPerLevel; }

        public static Room GetCurrentRoom { get => GameDataBoundManager.GetDungeonData.CurrentRoom; }
        /// <summary>
        /// Returns each individual room data. The collection is ordered by Room number.
        /// </summary>
        public static Room[] RoomsData { get => GameDataBoundManager.GetDungeonData.RoomsData; }
        /// <summary>
        /// Returns each individual room progress. The collection is ordered by Room number.
        /// </summary>
        public static RoomProgress[] RoomsProgress { get => PlayerDataBoundManager.GetDungeonProgress.roomsProgress; }
        /// <summary>
        /// Returns each individual room unlockables. The collection is ordered by Room number.
        /// </summary>
        public static RoomUnlockables[] RoomUnlockables { get => GameDataBoundManager.GetDungeonData.RoomsUnlockablesData; }
        public static AnimatedSexSceneData[] AnimatedScenesData { get => GameDataBoundManager.GetDungeonData.ScenesData; }
        /// <summary>
        /// Reference to the room display.
        /// </summary>
        public static RoomView RoomDisplay { private get; set; }

        private UnityAction<List<ItemInstance>> onInventoryUpdate;
        private DialogueSetup.OnDialogueFinished onDungeonDialogeEnded;
        private Action onDungeonSceneEnded;

        /// <summary>
        /// Displays a room.
        /// </summary>
        /// <param name="_roomToDisplay">Room to display.</param>
        public static void DisplayRoom(Room _roomToDisplay)
        {
            GameDataBoundManager.SetCurrentRoom(_roomToDisplay);

            OnRoomSelected?.Invoke(_roomToDisplay);
        }

        #region Item management      
        public static void RequestItemUpgrade(int _roomNumber, string _itemId, int _levelsToIncrease)
        {
            if (HasCoinsForUpgrade(_roomNumber, _itemId, _levelsToIncrease))
            {
                RoomItem item = GetRoomItemProgress(_roomNumber, _itemId);
                if (!CanUpgradeItem(_roomNumber, item.ItemId, _levelsToIncrease))
                {
                    GenericPopup.Show("Affection test pending!", $"There's a pending affection test for { item.GetName() }, unlock it before continuing.");
                }
                // If there is no unlockable pending or the unlockable is already unlocked we can upgrade.
                else
                {
                    instance.ProcessItemUpgrade(_roomNumber, item, _levelsToIncrease);
                }
            }
            else
            {
                // TODO show bring all items pop-up
                GenericPopup.Show("Not enough silver!", "Get more silver before trying to upgrade this item.");
            }
        }

        private void ProcessItemUpgrade(int _roomNumber, RoomItem _itemToUpgrade, int _levelsToIncrease)
        {
            if (_itemToUpgrade.CanLevelUp())
            {
                int upgradeCost = (_itemToUpgrade.ItemLvl + _levelsToIncrease) * GameDataBoundManager.GetDungeonData.UpgradeCostPerLevel;
                int currentCoins = PlayerInfo.instance.Coins;

                // Callback to chain after the inventory has been updated.
                onInventoryUpdate = (inventory) =>
                {
                    _itemToUpgrade.UpgradeItem(_levelsToIncrease);

                    //StoreRoomItemProgress(GetCurrentRoom.RoomNumber, _itemToUpgrade);

                    GameStateManager.instance.StartWait("Saving progress.");

                    PlayerDataBoundManager.SaveDungeonProgress(null,
                    () =>
                    {
                        Debugging.DebugController.LogErrorMessage($"Error while upgrading item { _itemToUpgrade.ItemId }");
                    });

                    PFWrapper.OnItemDataUpdated -= onInventoryUpdate;
                };

                PFWrapper.OnItemDataUpdated += onInventoryUpdate;

                GameStateManager.instance.StartWait("Upgrading item.");

                PlayerInfo.instance.Coins = (int)(currentCoins - upgradeCost);
            }
        }

        /// <summary>
        /// Requests an upgrade for multiple room items.
        /// </summary>
        /// <param name="_roomNumber">Number of the current room.</param>
        /// <param name="_itemsId">Ids of the items to upgrade.</param>
        public static void RequestMultipleItemsUpgrade(int _roomNumber, string[] _itemsId)
        {
            int upgradeAllCost = GetUpgradeAllCost(_roomNumber, _itemsId);
            int currentCoins = PlayerInfo.instance.Coins;
            if (currentCoins >= upgradeAllCost)
            {
                RoomItem[] itemsToUpgrade = new RoomItem[_itemsId.Length];
                for (int i = 0; i < _itemsId.Length; i++)
                {
                    itemsToUpgrade[i] = GetRoomItemProgress(_roomNumber, _itemsId[i]);
                }
                instance.ProcessMultipleItemsUpgrade(_roomNumber, itemsToUpgrade);
            }
            else
            {
                GenericPopup.Show("Not enough silver!", "Not enough silver to upgrade all items.");
            }
        }

        /// <summary>
        /// Processes multiple room items upgrade.
        /// </summary>
        /// <param name="_roomNumber">Numbr of the room the items belong to.</param>
        /// <param name="_itemsToUpgrade">Items to upgrade.</param>
        private void ProcessMultipleItemsUpgrade(int _roomNumber, RoomItem[] _itemsToUpgrade)
        {
            int upgradeCost = 0;
            int currentCoins = PlayerInfo.instance.Coins;
            Array.ForEach(_itemsToUpgrade, (item) => upgradeCost += GetItemUpgradeCost(_roomNumber, item.ItemId, 1));

            // Callback to chain after the inventory has been updated.
            onInventoryUpdate = (inventory) =>
            {
                for (int i = 0; i < _itemsToUpgrade.Length; i++)
                {
                    _itemsToUpgrade[i].UpgradeItem();
                    //StoreRoomItemProgress(GetCurrentRoom.RoomNumber, _itemsToUpgrade[i]);
                }

                SaveDungeonProgress(null, null);

                PFWrapper.OnItemDataUpdated -= onInventoryUpdate;
            };

            PFWrapper.OnItemDataUpdated += onInventoryUpdate;

            GameStateManager.instance.StartWait("Upgrading items.");

            PlayerInfo.instance.Coins = (int)(currentCoins - upgradeCost);
        }

        /// <summary>
        /// Saves the current progress.
        /// </summary>
        /// <param name="_onSucess">Callback to execute on save success.</param>
        /// <param name="_onError">Callback to execute on save error.</param>
        public static void SaveDungeonProgress(Action _onSucess, Action _onError)
        {
            PlayerDataBoundManager.SaveDungeonProgress(() =>
            {
                _onSucess?.Invoke();
            },
            () =>
            {
                Debugging.DebugController.LogErrorMessage($"Error while saving progress.");
                _onError?.Invoke();
            });
        }

        /// <summary>
        /// Unlocks a dungeon item.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the item belongs to.</param>
        /// <param name="_itemId">Id of the item to unlock.</param>
        public static void UnlockItem(int _roomNumber, string _itemId)
        {
            RoomItem item = GetRoomItemProgress(_roomNumber, _itemId);
            if (HasEnoughItemsToUnlock(_itemId))
            {      
                instance.UnlockRoomItem(_roomNumber, item);
            }
            else
            {
                // TODO show bring all items pop-up
                GenericPopup.Show("Not enough items in inventory", $"You don't have enough items to unlock { item.GetName() }, get more in the oracle.");
            }
        }

        /// <summary>
        /// Unlocks a room item.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the item belongs to.</param>
        /// <param name="_itemToUnlock">Item to unlock.</param>
        private void UnlockRoomItem(int _roomNumber, RoomItem _itemToUnlock)
        {
            List<ItemInstance> inventory = PFWrapper.instance.GetPlayerInventory();
            ItemInstance matchedItem = inventory.SingleOrDefault(item => item.ItemId == _itemToUnlock.ItemId);

            GameStateManager.instance.StartWait("Unlocking item.");

            App.Repository.ConsumeDungeonItemFromInventory(matchedItem.ItemInstanceId, 1, () =>
            {
                _itemToUnlock.UnlockItem();

                //StoreRoomItemProgress(GetCurrentRoom.RoomNumber, _itemToUnlock);

                SaveDungeonProgress(null, () =>
                {
                    Debugging.DebugController.LogErrorMessage("An unexpected error ocurred while saving item.");
                });
            }, () =>
            {
                GameStateManager.instance.EndWait();
                GenericPopup.Show("Error", "An unexpected error ocurred while unlocking item.");
            });
        }
        #endregion

        #region Unlockables management
        /// <summary>
        /// Unlock a specific room unlockable.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_unlockableLevel">Level of the unlockable to unlock.</param>
        public static void UnlockDungeonUnlockable(int _roomNumber, int _unlockableLevel)
        {
            if (HasEnoughGifts(_roomNumber, GetUnlockableData(_roomNumber, _unlockableLevel).unlockRequirement))
            {
                instance.UnlockRoomUnlockable(_roomNumber, _unlockableLevel);
            }         
        }

        /// <summary>
        /// Unlock a specific room unlockable.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_unlockableLevel">Level of the unlockable to unlock.</param>
        public static void GiveDungeonUnlockableGifts(int _roomNumber, int _unlockableLevel, string _giftId, int _amountToGive)
        {
            if (HasEnoughGifts(_roomNumber, GetUnlockableData(_roomNumber, _unlockableLevel).unlockRequirement, _giftId, _amountToGive))
            {
                instance.GiveGifts(_roomNumber, _unlockableLevel, _giftId, _amountToGive);
            }
        }

        private void GiveGifts(int _roomNumber, int _unlockableLevel, string _giftId, int _amountToGive)
        {
            RoomUnlockableData unlockableToUnlock = GetUnlockableData(_roomNumber, _unlockableLevel);

            PayForGiftsProcess(unlockableToUnlock, _giftId, _amountToGive, () =>
            {
                RoomUnlockableProgress unlockableProgress = GetRoomUnlockableProgress(_roomNumber, _unlockableLevel);
                unlockableProgress.GiveGifts(_giftId, _amountToGive);

                SaveDungeonProgress(() =>
                {
                    if (unlockableProgress.IsUnlocked)
                    {
                        ShowAfectionReward(unlockableToUnlock);
                    }
                },
                () =>
                {
                    Debugging.DebugController.LogErrorMessage("An unexpected error ocurred while saving unlockable progress.");
                });
            },
            () =>
            {
                Debugging.DebugController.LogErrorMessage("An unexpected error ocurred while giving gifts to princess.");
            });
        }

        /// <summary>
        /// Pays for an affection reward with gifts.
        /// </summary>
        /// <param name="_unlockable">Unlockable to pay for.</param>
        /// <param name="_onSucess">Callback to execute on save success.</param>
        /// <param name="_onError">Callback to execute on save error.</param>
        private void PayForGiftsProcess(RoomUnlockableData _unlockable, string _giftId, int _amountToGive, Action _onSucess, Action _onError)
        {
            List<string> itemIds = new List<string>();
            List<int> amounts = new List<int>();

            List<ItemInstance> inventory = PFWrapper.instance.GetPlayerInventory();
            foreach (RoomGiftRequirement gift in _unlockable.unlockRequirement)
            {
                if (gift.giftId.Equals(_giftId))
                {
                    ItemInstance matchedItem = inventory.SingleOrDefault(item => item.ItemId.Equals(gift.giftId));
                    if (matchedItem != null)
                    {
                        itemIds.Add(matchedItem.ItemInstanceId);
                        amounts.Add(_amountToGive);

                        break;
                    }
                }
            }

            GameStateManager.instance.StartWait("Giving gifts to princess.");

            App.Repository.ConsumeMultipleItemsFromInventory(itemIds.ToArray(), amounts.ToArray(), () =>
            {
                GameStateManager.instance.EndWait();
                _onSucess?.Invoke();
            },
            () =>
            {
                GameStateManager.instance.EndWait();
                _onError?.Invoke();
            });
        }

        /// <summary>
        /// Unlocks a room unlockable.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_unlockableLevel">Level of the unlockable.</param>
        private void UnlockRoomUnlockable(int _roomNumber, int _unlockableLevel)
        {
            RoomUnlockableData unlockableToUnlock = GetUnlockableData(_roomNumber, _unlockableLevel);

            PayForUnlockableRequirementsProcess(unlockableToUnlock, () =>
            {
                RoomUnlockableProgress unlockableProgress = GetRoomUnlockableProgress(_roomNumber, _unlockableLevel);
                unlockableProgress.UnlockUnlockable();

                SaveDungeonProgress(() =>
                {
                    ShowAfectionReward(unlockableToUnlock);
                },
                () =>
                {
                    GenericPopup.Show("Error", "An unexpected error ocurred while saving unlockable progress.");
                });
            },
            () =>
            {
                GenericPopup.Show("Error", "An unexpected error ocurred while giving gifts to princess.");
            });
        }

        /// <summary>
        /// Pays for an affection reward with gifts.
        /// </summary>
        /// <param name="_unlockable">Unlockable to pay for.</param>
        /// <param name="_onSucess">Callback to execute on save success.</param>
        /// <param name="_onError">Callback to execute on save error.</param>
        private void PayForUnlockableRequirementsProcess(RoomUnlockableData _unlockable, Action _onSucess, Action _onError)
        {
            List<string> itemIds = new List<string>();
            List<int> amounts = new List<int>();

            List<ItemInstance> inventory = PFWrapper.instance.GetPlayerInventory();
            Array.ForEach(_unlockable.unlockRequirement, (gift) =>
            {
                ItemInstance matchedItem = inventory.SingleOrDefault(item => item.ItemId.Equals(gift.giftId));
                if (matchedItem != null)
                {
                    itemIds.Add(matchedItem.ItemInstanceId);
                    amounts.Add(gift.amount);
                }
            });

            GameStateManager.instance.StartWait("Giving gifts to princess.");

            App.Repository.ConsumeMultipleItemsFromInventory(itemIds.ToArray(), amounts.ToArray(), () =>
            {
                GameStateManager.instance.EndWait();
                _onSucess?.Invoke();
            },
            () =>
            {
                GameStateManager.instance.EndWait();
                _onError?.Invoke();
            });
        }

        /// <summary>
        /// Stores a room unlockable progress.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_unlockableLevel">Level of the unlockable.</param>
        public void StoreRoomUnlockableProgress(int _roomNumber, int _unlockableLevel)
        {
            RoomProgress room = GetRoomProgress(_roomNumber);

            RoomUnlockableProgress unlockableProgress;
            unlockableProgress = GetRoomUnlockableProgress(_roomNumber, _unlockableLevel);

            room.StoreUnlockableProgress(unlockableProgress);
            PlayerDataBoundManager.StoreRoomProgress(room);
        }
        #endregion

        #region Data handling
        /// <summary>
        /// Get a specific room unlockable data. Can return if there is no unlockable for that level.
        /// </summary>
        /// <param name="_roomNumber">The number of the room the unlockable belongs too.</param>
        /// <param name="_unlockableLevel">The unlock level of the unlockable.</param>
        /// <returns></returns>
        public static RoomUnlockableData GetUnlockableData(int _roomNumber, int _unlockableLevel)
        {
            RoomUnlockableData data = GetRoomUnlockables(_roomNumber).unlockables.SingleOrDefault(matchingUnlockable => matchingUnlockable.unlockLevel == _unlockableLevel);
            return data;
        }

        /// <summary>
        /// Get a specific room unlockables.
        /// </summary>
        /// <param name="_roomNumber">Number of the room to get the unlockables.</param>
        /// <returns></returns>
        public static RoomUnlockables GetRoomUnlockables(int _roomNumber)
        {
            return GameDataBoundManager.GetRoomUnlockables(_roomNumber);
        }

        /// <summary>
        /// Get a specific room progress.
        /// </summary>
        /// <param name="_roomNumber">Number of the room to get the progress.</param>
        /// <returns></returns>
        public static RoomProgress GetRoomProgress(int _roomNumber)
        {
            return PlayerDataBoundManager.GetRoomProgress(_roomNumber);
        }

        /// <summary>
        /// Gets the current process for a specific unlockable in a specific room. Can return null if there is no progress.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs to.</param>
        /// <param name="_unlockableLevel">Level of the unlockable to check for progress.</param>
        /// <returns></returns>
        public static RoomUnlockableProgress GetRoomUnlockableProgress(int _roomNumber, int _unlockableLevel)
        {
            return PlayerDataBoundManager.GetRoomUnlockableProgress(_roomNumber, _unlockableLevel);
        }

        /// <summary>
        /// Gets a room item progress.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_itemId">Id of the item to get the progress.</param>
        /// <returns></returns>
        public static RoomItem GetRoomItemProgress(int _roomNumber, string _itemId)
        {
            RoomItem progressToReturn = null;
            var roomProgress = GetRoomProgress(_roomNumber);
            progressToReturn = roomProgress.ItemsProgress.SingleOrDefault(itemProgress => itemProgress.ItemId.Equals(_itemId));
            return progressToReturn;
        }

        /// <summary>
        /// Gets an item upgrade cost.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_itemId">Id of the item to get the cost.</param>
        /// <returns></returns>
        public static int GetItemUpgradeCost(int _roomNumber, string _itemId, int _levelsToIncrease)
        {
            RoomItem item = GetRoomItemProgress(_roomNumber, _itemId);
            int cost = 0;
            if (item != null)
            {
                for (int i = 0; i < _levelsToIncrease; i++)
                {
                    cost += UpgradeCostPerLevel * (item.ItemLvl + i);
                }
            }
            return cost;
        }

        /// <summary>
        /// Gets the cost for upgrading all items.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs.</param>
        /// <param name="_itemsId">Ids of the items to get the cost.</param>
        /// <returns></returns>
        public static int GetUpgradeAllCost(int _roomNumber, string[] _itemsId)
        {
            int cost = 0;
            Array.ForEach(_itemsId, item => cost += GetItemUpgradeCost(_roomNumber, item, 1));
            return cost;
        }

        /// <summary>
        /// Finds the first quest number for a given reward.
        /// </summary>
        /// <param name="_roomNumber">Number of the current room. Rooms are in same order as the books.</param>
        /// <param name="_rewardId">Id for the reward to look for.</param>
        /// <returns></returns>
        public static int GetPosibleRewardQuestNumber(int _roomNumber, string _rewardId, out string _difficultLocation)
        {
            // This is posible because the gifts required for a certain room number are obtain in the book of the same number.
            BookRewardsBound bookRewards = QuestManagerBound.BoundInstance.BookRewards[_roomNumber - 1];
            QuestRewardsBound[] questsToLook;
            if (Regex.Match(_rewardId, @"gold").Success)
            {
                //Search hard quests.
                questsToLook = bookRewards.HardQuestsRewards;
                _difficultLocation = LocalizationHelper.GetLocalizedTerm("MainMenu/tid_MainMenu_Hard");
            }
            else
            {
                //Search normal quests.
                questsToLook = bookRewards.NormalQuestsRewards;
                _difficultLocation = LocalizationHelper.GetLocalizedTerm("MainMenu/tid_MainMenu_Normal");
            }

            for (int i = 0; i < questsToLook.Length; i++)
            {
                foreach (var reward in questsToLook[i].Rewards)
                {
                    //RewardBound reward = questsToLook[i].Rewards.SingleOrDefault(matchingReward => matchingReward.Reward.Id.Equals(_rewardId));
                    if (reward.Reward.Id.Equals(_rewardId))
                    {
                        return i + 1;
                    }
                }
            }

            throw new NoMatchingQuestException(_rewardId);
        }

        /// <summary>
        /// Gets a room level.
        /// </summary>
        /// <param name="_roomNumber">Number room for getting the level.</param>
        /// <returns></returns>
        public static int GetRoomLevel(int _roomNumber)
        {
            RoomProgress room = GetRoomProgress(_roomNumber);
            int minLevel = room.ItemsProgress.Min(item => item.ItemLvl);

            return minLevel;
        }
        #endregion

        public static void ShowAfectionReward(RoomUnlockableData _unlockableToUnlock)
        {
            instance.StoreRoomUnlockableProgress(GetCurrentRoom.RoomNumber, _unlockableToUnlock.unlockLevel);

            instance.StartCoroutine(instance.ShowReward(_unlockableToUnlock));
        }

        private IEnumerator ShowReward(RoomUnlockableData _unlockeable)
        {
            bool canContinue = false;

            if (!string.IsNullOrEmpty(_unlockeable.dialogueId))
            {
                RoomDisplay.SetUI(false);
                DialogueInfo dialogueInfo = DialogueSetupBound.instance.GetDialogueInfo(_unlockeable.dialogueId);
                DungeonDialogue.Instance.Open(dialogueInfo);

                onDungeonDialogeEnded = (dialogueId) =>
                {
                    RoomDisplay.SetUI(true);

                    OnUnlockableUnlock();

                    canContinue = true;

                    DungeonDialogue.Instance.DialogueFinished -= onDungeonDialogeEnded;
                };

                DungeonDialogue.Instance.DialogueFinished += onDungeonDialogeEnded;
            }
            else 
            {
                canContinue = true;
            } 

            yield return new WaitUntil(() => canContinue == true);
            //canContinue = false;

            if (!string.IsNullOrEmpty(_unlockeable.sexSceneId))
            {
                GameObject sceneInstance = Instantiate(App.Resources.GetAssetFromResources<GameObject>("AnimatedSexScenes/" + _unlockeable.sexSceneId), animatedSceneCanvas);

                onDungeonSceneEnded = () =>
                {
                    OnUnlockableUnlock();

                    sceneInstance.GetComponent<AnimatedSceneManager>().OnEndScene -= onDungeonSceneEnded;
                };

                sceneInstance.GetComponent<AnimatedSceneManager>().OnEndScene += onDungeonSceneEnded;

                if (PlayerDataBoundManager.playerData.sexScenesSeen.ContainsKey(_unlockeable.sexSceneId))
                {
                    PlayerDataBoundManager.playerData.sexScenesSeen[_unlockeable.sexSceneId] = true; 
                    PFWrapper.instance.UpdateBoundUserData("SexScenesSeen", PFWrapper.instance.SerializeData(PlayerDataBoundManager.playerData.sexScenesSeen), null, null);
                }
            }
        }

        #region Conditionals

        /// <summary>
        /// Check if the player has enough coins to upgrade
        /// </summary>
        /// <param name="_item">Item to upgrade.</param>
        /// <returns></returns>
        public static bool HasCoinsForUpgrade(int _roomNumber, string _itemId, int _levelsToIncrease)
        {
            bool hasCoins = false;
            int currentCoins = PlayerInfo.instance.Coins;
            int upgradeCost = GetItemUpgradeCost(_roomNumber, _itemId, _levelsToIncrease);
            if (currentCoins >= upgradeCost)
            {
                hasCoins = true;
            }

            return hasCoins;
        }

        /// <summary>
        /// Checks if the item can upgraded.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the item belongs to.</param>
        /// <param name="_itemId">Id of the item to check.</param>
        /// <returns></returns>
        public static bool CanUpgradeItem(int _roomNumber, string _itemId)
        {
            RoomItem itemToCheck = GetRoomItemProgress(_roomNumber, _itemId);
            bool canLevelUp = itemToCheck.CanLevelUp() && !IsThereAUnlockablePending(_roomNumber, itemToCheck.ItemLvl);
            return canLevelUp;
        }

        /// <summary>
        /// Checks if the item can upgraded.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the item belongs to.</param>
        /// <param name="_itemId">Id of the item to check.</param>
        /// <param name="_levelsToIncrease">Number of levels to increase.</param>
        /// <returns></returns>
        public static bool CanUpgradeItem(int _roomNumber, string _itemId, int _levelsToIncrease)
        {
            RoomItem itemToCheck = GetRoomItemProgress(_roomNumber, _itemId);
            bool isMaxLevel = !itemToCheck.CanLevelUp(_levelsToIncrease);
            bool canLevelUp = true;
            for (int i = 0; i < _levelsToIncrease; i++)
            {
                canLevelUp = !IsThereAUnlockablePending(_roomNumber, itemToCheck.ItemLvl + i);
                if (!canLevelUp)
                {
                    break;
                }
            }

            return !isMaxLevel && canLevelUp;
        }

        /// <summary>
        /// Check if there's a unlockable pending for a specific level.
        /// </summary>
        /// <param name="_itemLevel">Unlockable level.</param>
        /// <returns></returns>
        public static bool IsThereAUnlockablePending(int _roomNumber, int _levelToCheck)
        {
            bool unlockablePending = false;
            RoomUnlockableData unlockablePendingData = GetUnlockableData(_roomNumber, _levelToCheck);
            RoomUnlockableProgress unlockableProgress = GetRoomUnlockableProgress(_roomNumber, _levelToCheck);
            // If there is a unlockable pending check if it's unlocked.
            if (unlockablePendingData != null && (unlockableProgress == null || !unlockableProgress.IsUnlocked))
            {
                unlockablePending = true;
            }

            return unlockablePending;
        }

        /// <summary>
        /// Check if the player has enough items in the inventory to upgrade.
        /// </summary>
        /// <param name="_item">Room item to unlock.</param>
        /// <returns></returns>
        public static bool HasEnoughItemsToUnlock(string _itemId)
        {
            bool hasItems = false;
            List<ItemInstance> inventory = PFWrapper.instance.GetPlayerInventory();
            ItemInstance matchedItem = inventory.SingleOrDefault(item => item.ItemId == _itemId);
            if (matchedItem != null && matchedItem.RemainingUses >= 1)
            {
                hasItems = true;
            }

            return hasItems;
        }

        /// <summary>
        /// Checks if the player has enought amount per gift in the inventory.
        /// </summary>
        /// <returns></returns>
        public static bool HasEnoughGifts(int _roomNumber, RoomGiftRequirement[] _requiredGifts)
        {
            bool hasGifts = true;
            List<ItemInstance> inventory = PFWrapper.instance.GetPlayerInventory();
            foreach (RoomGiftRequirement gift in _requiredGifts)
            {
                ItemInstance matchedItem = inventory.SingleOrDefault(item => item.ItemId == gift.giftId);
                if (matchedItem == null || matchedItem.RemainingUses < gift.amount)
                {
                    string difficultLocation = string.Empty;
                    hasGifts = false;
                    GenericOkCancelPopup.Show(
                        string.Format("You don't have enough {2} {0} {1}",
                            gift.GetQualityName(),
                            gift.GetName(), gift.GetPrincessName()
                            ),
                        string.Format("You can find it at ({2}) Book {0} Quest {1}\n Would you like to go?", _roomNumber,
                            GetPosibleRewardQuestNumber(_roomNumber, gift.giftId, out difficultLocation),
                            difficultLocation),
                        "", "", 
                        () => NavigationManager.instance.FromDungeonToQuest(_roomNumber, GetPosibleRewardQuestNumber(_roomNumber, gift.giftId, out difficultLocation), difficultLocation)
                        );
                    break;
                }
            }

            return hasGifts;
        }

        /// <summary>
        /// Checks if the player has enought amount per gift in the inventory.
        /// </summary>
        /// <returns></returns>
        public static bool HasEnoughGifts(int _roomNumber, RoomGiftRequirement[] _requiredGifts, string _giftId, int _amountToGive)
        {
            bool hasGifts = true;
            List<ItemInstance> inventory = PFWrapper.instance.GetPlayerInventory();
            foreach (RoomGiftRequirement gift in _requiredGifts)
            {
                if (gift.giftId.Equals(_giftId))
                {
                    ItemInstance matchedItem = inventory.SingleOrDefault(item => item.ItemId == gift.giftId);
                    if (matchedItem == null || matchedItem.RemainingUses < _amountToGive)
                    {
                        string difficultLocation = string.Empty;
                        hasGifts = false;
                        GenericOkCancelPopup.Show(
                        string.Format("You don't have enough {2} {0} {1}",
                            gift.GetQualityName(),
                            gift.GetName(), gift.GetPrincessName()
                            ),
                        string.Format("You can find it at ({2}) Book {0} Quest {1}\n Would you like to go?", _roomNumber,
                            GetPosibleRewardQuestNumber(_roomNumber, gift.giftId, out difficultLocation),
                            difficultLocation),
                        "", "",
                        () => NavigationManager.instance.FromDungeonToQuest(_roomNumber, GetPosibleRewardQuestNumber(_roomNumber, gift.giftId, out difficultLocation), difficultLocation)
                        );
                    }
                    break;
                }
            }

            return hasGifts;
        }

        /// <summary>
        /// Checks if all the items for the current room are unlocked.
        /// </summary>
        /// <returns></returns>
        public static bool AreAllItemsUnlocked(int _roomNumber)
        {
            bool areUnlocked = true;
            RoomProgress room = GetRoomProgress(_roomNumber);
            foreach (RoomItem item in room.ItemsProgress)
            {
                if (!item.IsUnlocked)
                {
                    areUnlocked = false;
                    break;
                }
            }

            return areUnlocked;
        }
        #endregion

        private void OnUnlockableUnlock()
        {
            OnUnlockableEnded?.Invoke();
        }
    }
}
