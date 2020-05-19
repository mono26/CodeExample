using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Timba.Bound.Dungeon;
using Timba.Bound.GameUnlockables;

namespace Timba.Bound
{
    public class PlayerDataBoundManager : Singleton<PlayerDataBoundManager>
    {
        public static PlayerData playerData = new PlayerData();

        public static PlayerUnlockabesData GetUnlockablesProgress { get => playerData.unlockablesProgress; }
        public static PlayerDungeonProgress GetDungeonProgress { get => playerData.dungeonProgress; }

        private void Start()
        {
            PFWrapper.OnUserDataUpdated += InitializePlayerData;
        }

        private void InitializePlayerData(Dictionary<string, UserDataRecord> _userData)
        {
            playerData.InitializePlayerData(_userData, App.Repository);
        }

        #region Unlockables
        /// <summary>
        /// Marks the unlockable content as seen.
        /// </summary>
        /// <param name="_contentId">Content to mark as seen.</param>
        public static void SetUnlockableSeenState(UnlockableContent _contentId, bool _seen)
        {
            playerData.unlockablesProgress.SetContentSeenState(_contentId, true);
        }

        /// <summary>
        /// Sets the room seen state.
        /// </summary>
        /// <param name="_room">Room to set.</param>
        /// <param name="_seen">Seen state.</param>
        public static void SetRoomSeenState(string _room, bool _seen)
        {
            playerData.unlockablesProgress.SetRoomSeenState(_room, _seen);
        }

        /// <summary>
        /// Sets the gallery item seen state.
        /// </summary>
        /// <param name="_scene">Gllery item to set.</param>
        /// <param name="_seen">Seen state.</param>
        public static void SetGalleryItemseenState(string _scene, bool _seen)
        {
            playerData.unlockablesProgress.SetGalleryItemseenState(_scene, _seen);
        }

        /// <summary>
        /// Sets the gallery item seen state.
        /// </summary>
        /// <param name="_scene">Gllery item to set.</param>
        /// <param name="_seen">Seen state.</param>
        public static void SetContentSeenState(UnlockableContent _contentId, bool _seen)
        {
            playerData.unlockablesProgress.SetContentSeenState(_contentId, _seen);
        }

        /// <summary>
        /// Unlocks the specified unlocakble content.
        /// </summary>
        /// <param name="_contentId">Content to unlock.</param>
        public static void SetUnlockableContentUnlockState(UnlockableContent _contentId, bool _unlocked)
        {
            playerData.unlockablesProgress.SetUnlockableContentUnlockState(_contentId, _unlocked);
        }

        /// <summary>
        /// Saves the player current unlockables data.
        /// </summary>
        /// <param name="_onSuccess">Callback on success.</param>
        /// <param name="_onError">Callback on error.</param>
        public static void SavePlayerUnlockablesData(Action _onSuccess, Action _onError)
        {
            playerData.unlockablesProgress.SaveUnlockedContent(App.Repository, _onSuccess, _onError);
        }

        /// <summary>
        /// Handles getting a room seen state.
        /// </summary>
        /// <param name="_room">Room to get.</param>
        /// <returns></returns>
        public static bool GetRoomSeenState(string _room)
        {
            return playerData.unlockablesProgress.GetRoomSeenState(_room);
        }

        /// <summary>
        /// Handles getting a gallery item seen state.
        /// </summary>
        /// <param name="_galleryItem"></param>
        /// <returns></returns>
        public static bool GetGalleryItemSeenState(string _galleryItem)
        {
            return playerData.unlockablesProgress.GetGalleryItemSeenState(_galleryItem);
        }
        #endregion

        #region Dungeon
        /// <summary>
        /// Get a specific room progress.
        /// </summary>
        /// <param name="_roomNumber">Number of the room to get the progress.</param>
        /// <returns></returns>
        public static RoomProgress GetRoomProgress(int _roomNumber)
        {
            RoomProgress progress = playerData.dungeonProgress.roomsProgress?.SingleOrDefault(matchingProgress => matchingProgress.RoomNumber == _roomNumber);
            if (progress == null)
            {
                progress = playerData.dungeonProgress.CreateNewRoomProgressData(_roomNumber);
            }
            return progress;
        }

        /// <summary>
        /// Gets the current process for a specific unlockable in a specific room. Can return null if there is no progress.
        /// </summary>
        /// <param name="_roomNumber">Number of the room the unlockable belongs to.</param>
        /// <param name="_unlockableLevel">Level of the unlockable to check for progress.</param>
        /// <returns></returns>
        public static RoomUnlockableProgress GetRoomUnlockableProgress(int _roomNumber, int _unlockableLevel)
        {
            RoomUnlockableProgress progress = GetRoomProgress(_roomNumber)?.UnlockablesProgress?.SingleOrDefault(matchingUnlockable => matchingUnlockable.UnlockLevel == _unlockableLevel);
            return progress;
        }

        /// <summary>
        /// Handles the store room progress in to the dungeon progress data.
        /// </summary>
        /// <param name="_roomToStore"></param>
        public static void StoreRoomProgress(RoomProgress _roomToStore)
        {
            playerData.dungeonProgress.StoreRoomProgress(_roomToStore);
        }

        /// <summary>
        /// Handles the dungeon progress save into the server.
        /// </summary>
        /// <param name="_onSuccess"></param>
        /// <param name="_onError"></param>
        public static void SaveDungeonProgress(Action _onSuccess, Action _onError)
        {
            playerData.dungeonProgress.SaveDungeonProgress(App.Repository, _onSuccess, _onError);
        }
        #endregion
    }
}
