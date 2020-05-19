using System.Collections;
using System.Collections.Generic;
using Timba.Bound.Dungeon;
using UnityEngine;

namespace Timba.Bound
{
    public class GameDataBoundManager : Singleton<GameDataBoundManager>
    {
        public static DungeonData dungeonData = new DungeonData();

        public static DungeonData GetDungeonData { get => dungeonData; }

        private void Start()
        {
            PFWrapper.OnTitleDataUpdated += InitalizeGameData;
        }

        private void OnDestroy()
        {
            PFWrapper.OnTitleDataUpdated -= InitalizeGameData;
        }

        public void InitalizeGameData()
        {
            dungeonData.InitializeData(App.Repository);
        }

        #region Dungeon
        /// <summary>
        /// Handles the current room in the dungeon.
        /// </summary>
        /// <param name="_currentRoom"></param>
        public static void SetCurrentRoom(Room _currentRoom)
        {
            dungeonData.SetCurrentRoom(_currentRoom);
        }

        /// <summary>
        /// Get a specific room unlockables.
        /// </summary>
        /// <param name="_roomNumber">Number of the room to get the unlockables.</param>
        /// <returns></returns>
        public static RoomUnlockables GetRoomUnlockables(int _roomNumber)
        {
            return dungeonData.GetUnlockablesData(_roomNumber);
        }
        #endregion
    }
}
