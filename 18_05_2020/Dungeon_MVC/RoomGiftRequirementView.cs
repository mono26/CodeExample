using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Timba.Bound.Dungeon
{
    public class RoomGiftRequirementView : DungeonSubpanel
    {
        [SerializeField]
        private HorizontalScrollSnap pagesScroll = null;
        [SerializeField]
        private RoomGiftRequirementPage pagePrefab = null;

        private int currentPage = -1;

        private void Start()
        {
            RoomUnlockableProgress.OnRoomUnlockableUnlocked += OnRoomUnlockableUnlocked;
        }

        private void OnDestroy()
        {
            RoomUnlockableProgress.OnRoomUnlockableUnlocked += OnRoomUnlockableUnlocked;
        }

        private void OnRoomUnlockableUnlocked(int _unlockLevel)
        {
            Close();
        }

        public void InitializeView(RoomGiftRequirement[] _gift)
        {
            GameObject[] children;
            pagesScroll.RemoveAllChildren(out children);
            foreach (GameObject child in children)
            {
                Destroy(child);
            }

            for (int i = 0; i < _gift.Length; i++)
            {
                RoomGiftRequirementPage page = Instantiate(pagePrefab);
                page.InitializePage(_gift[i]);
                pagesScroll.AddChild(page.gameObject);
            }
            gameObject.SetActive(false);
        }

        public void GoToPage(int _page)
        {
            currentPage = _page;

            gameObject.SetActive(true);

            pagesScroll.ChangePage(_page);
            pagesScroll.UpdateLayout();

            StartCoroutine(SetState(true));
        }

        /// <summary>
        /// Displays a gift's icon inside a image component.
        /// </summary>
        /// <param name="_gift">Gift to diplay.</param>
        /// <param name="_imageComponent">Icon holder.</param>
        public static void DisplayGiftImage(DungeonGift _gift, Image _imageComponent)
        {
            Sprite giftIcon = _gift.GetImage();
            if (giftIcon)
            {
                _imageComponent.sprite = giftIcon;
            }
            else
            {
                Debug.LogError($"Missing gift sprite for: { _gift.ItemId }");
            }
        }

        /// <summary>
        /// Displays a gift's fable icon inside a image component.
        /// </summary>
        /// <param name="_gift">Gift to display the fable.</param>
        /// <param name="_imageComponent">Icon holder.</param>
        public static void DisplayGiftFable(DungeonGift _gift, Image _imageComponent)
        {
            Sprite fableImage = _gift.GetFableImage();
            if (fableImage)
            {
                _imageComponent.sprite = fableImage;
            }
            else
            {
                Debug.LogError($"Missing fable sprite for: { _gift.ItemId }");
            }
        }

        /// <summary>
        /// Displays a gift's quality frame icon inside a image component.
        /// </summary>
        /// <param name="_gift">Gift to display the quality frame.</param>
        /// <param name="_imageComponent">Icon holder.</param>
        public static void DisplayGiftQuality(DungeonGift _gift, Image _imageComponent)
        {
            Sprite giftImage = _gift.GetQualityImage();
            if (giftImage)
            {
                _imageComponent.sprite = _gift.GetQualityImage();
            }
            else
            {
                Debug.LogError($"Missing frame quality sprite for: { _gift.ItemId }");
            }
        }

        /// <summary>
        /// Displays text information about the amount given and required of a gift.
        /// </summary>
        /// <param name="_given">Given amount.</param>
        /// <param name="_required">Required amount.</param>
        /// <param name="_textComponent">Text holder.</param>
        public static void DisplayGivenAndRequired(int _given, int _required, TextMeshProUGUI _textComponent) => _textComponent.SetText(string.Format("{0}/{1}", _given, _required));

        void AnimMoveOutHandler() { }
    }
}
