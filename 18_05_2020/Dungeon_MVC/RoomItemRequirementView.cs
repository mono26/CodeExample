using System;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Timba.Bound.Dungeon
{
    /// <summary>
    /// MonoBehaviour class used to display a specific room item.
    /// </summary>
    public class RoomItemRequirementView : DungeonSubpanel
    {
        public static event Action OnItemViewOpen;
        public static event Action OnItemViewClose;

        [SerializeField]
        private HorizontalScrollSnap pagesScroll = null;
        [SerializeField]
        private RoomItemRequirementPage pagePrefab = null;

        private int currentPage = -1;

        public void InitilizeView(RoomItem[] _items)
        {
            currentPage = 0;

            GameObject[] children;
            pagesScroll.RemoveAllChildren(out children);
            foreach (GameObject child in children)
            {
                Destroy(child);
            }

            for (int i = 0; i < _items.Length; i++)
            {
                RoomItemRequirementPage page = Instantiate(pagePrefab);
                page.InitializePage(_items[i]);
                pagesScroll.AddChild(page.gameObject);
            }
            gameObject.SetActive(false);
        }

        public void OpenPage(int _page)
        {
            currentPage = _page;

            gameObject.SetActive(true);

            pagesScroll.ChangePage(_page);
            pagesScroll.UpdateLayout();

            StartCoroutine(SetState(true));

            OnItemViewOpen?.Invoke();
        }

        public static bool ShouldUpdateItemIcon(int _currentLevel, string _currentIconLevel) => !_currentIconLevel.Equals(GetIconLvl(_currentLevel));

        public static string GetIconLvl(int _currentLevel)
        {
            string lvl;
            if (_currentLevel >= 40)
            {
                lvl = "lvl3";
            }
            else if (_currentLevel >= 20)
            {
                lvl = "lvl2";
            }
            else
            {
                lvl = "lvl1";
            }

            return lvl;
        }

        public static int GetUpgradeCost(string _itemId, int _levelsToIncrease) => DungeonController.GetItemUpgradeCost(DungeonController.GetCurrentRoom.RoomNumber, _itemId, _levelsToIncrease);

        void AnimMoveOutHandler() { }

        public override void Close()
        {
            base.Close();

            OnItemViewClose?.Invoke();
        }
    }
}
