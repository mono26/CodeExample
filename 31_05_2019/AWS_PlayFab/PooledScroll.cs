using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScrollDirection
{
    TopToBottom, BottomToTop, LeftToight, RightToLeft
}

public enum SortingType
{
    None, FirstToLast, LastToFirst
}

[System.Serializable]
public class ElementSize
{
    [Range(0, 1)] public float verticalSizePercent;
    [Range(0, 1)] public float horizontalSizePercent;

    public float GetTargetHeight(float containerHeight)
    {
        return containerHeight * verticalSizePercent;
    }

    public float GetTargetWidth(float containerWidth)
    {
        return containerWidth * horizontalSizePercent;
    }
}

[System.Serializable]
public class ScrollData
{
    public string userId;
    public UserImageCollection imgsData;
}

public class PooledScroll : MonoBehaviour
{
    [SerializeField] ScrollDirection scrollingDirection =  ScrollDirection.TopToBottom;
    [SerializeField] ScrollRect scrollComponent = null;
    [SerializeField] VerticalLayoutGroup layoutGroupComponent = null;
    [SerializeField] protected ImageResult imagePFB = null;
    [SerializeField] protected ScrollData scrollData = null;
    [SerializeField] RectOffset scrollPadding = null;
    [SerializeField] ElementSize eleSize = null;
    [SerializeField] int numberOfCulledElements = 0;
    [SerializeField] List<ImageResult> activeElements =  new List<ImageResult>();
    [SerializeField] LayoutElement fakeCulledObjects = null;

    Vector2 lastScrollPosition = Vector2.zero;

#region Properties
    int GetNumberOfData { get { return scrollData.imgsData.imgCollection.Length; } }
    int GetNumberOfActiveElements { get { return activeElements.Count; } }
    float GetTopSortPosition
    {
        get
        {
            float scrollingPosition = 0;
            float scrollHeight = ((RectTransform)scrollComponent.transform).rect.height;
            float scrollWidth = ((RectTransform)scrollComponent.transform).rect.width;
            float scrollElementHeight = eleSize.GetTargetHeight(scrollHeight);
            float scrollElementWidth = eleSize.GetTargetWidth(scrollWidth);

            scrollingPosition = (scrollComponent.vertical) ? scrollElementHeight + scrollPadding.top + (numberOfCulledElements * scrollElementHeight) + layoutGroupComponent.spacing : scrollElementWidth + scrollPadding.left + (numberOfCulledElements * scrollElementWidth) + (((numberOfCulledElements - 1) * layoutGroupComponent.spacing));

            if (scrollingDirection == ScrollDirection.BottomToTop || scrollingDirection == ScrollDirection.LeftToight)
            {
                scrollingPosition = -scrollingPosition;
            }

            Vector2 pointA;
            Vector2 pointB;
            if (scrollComponent.vertical)
            {
                pointA = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(eleSize.GetTargetWidth(scrollWidth)/2, scrollingPosition + scrollHeight/2));
                pointB = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(-eleSize.GetTargetWidth(scrollWidth)/2, scrollingPosition + scrollHeight/2));
            }
            else
            {
                pointA = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(eleSize.GetTargetHeight(scrollHeight), scrollingPosition + scrollWidth/2));
                pointB = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(-eleSize.GetTargetHeight(scrollHeight), scrollingPosition + scrollWidth/2));
            }
            Debug.DrawLine(pointA, pointB, Color.cyan, 3.0f);
            
            return scrollingPosition;
        }
    }
    float GetBotSortPosition
    {
        get
        {
            float scrollingPosition = 0;
            float scrollHeight = ((RectTransform)scrollComponent.transform).rect.height;
            float scrollWidth = ((RectTransform)scrollComponent.transform).rect.width;
            float scrollElementHeight = eleSize.GetTargetHeight(scrollHeight);
            float scrollElementWidth = eleSize.GetTargetWidth(scrollWidth);
            var scrollRec = (RectTransform)scrollComponent.transform;

            scrollingPosition = (scrollComponent.vertical) ? scrollRec.rect.height + (scrollElementHeight + (layoutGroupComponent.spacing*2)) : scrollRec.rect.width + ((scrollElementWidth + (layoutGroupComponent.spacing*2))*2);

            if (numberOfCulledElements == GetNumberOfData - GetNumberOfActiveElements)
            {
                scrollingPosition -= ((scrollComponent.vertical) ? scrollElementWidth : scrollElementWidth) + (layoutGroupComponent.spacing*2);
            }

            if (scrollingDirection == ScrollDirection.BottomToTop || scrollingDirection == ScrollDirection.LeftToight)
            {
                scrollingPosition = -scrollingPosition;
            }

            scrollingPosition = GetTopSortPosition - scrollingPosition;

            Vector2 pointA;
            Vector2 pointB;
            if (scrollComponent.vertical)
            {
                pointA = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(eleSize.GetTargetWidth(scrollWidth)/2, scrollingPosition + scrollHeight/2));
                pointB = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(-eleSize.GetTargetWidth(scrollWidth)/2, scrollingPosition + scrollHeight/2));
            }
            else
            {
                pointA = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(eleSize.GetTargetHeight(scrollHeight), scrollingPosition + scrollWidth/2));
                pointB = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(-eleSize.GetTargetHeight(scrollHeight), scrollingPosition + scrollWidth/2));
            }
            Debug.DrawLine(pointA, pointB, Color.magenta, 3.0f);

            return scrollingPosition;
        }
    }
    float GetScrollTopPosition
    {
        get
        {
            float position = 0;
            var anchoredPosition = scrollComponent.content.anchoredPosition;
            position = (scrollComponent.vertical) ? anchoredPosition.y :  anchoredPosition.x;

            return position;
        }
    }
    float GetScrollBottomPosition
    {
        get
        {
            // Debug.Log("GetScrollBottomPosition");

            float position = 0;
            float scrollHeight = ((RectTransform)scrollComponent.transform).rect.height;
            float scrollWidth = ((RectTransform)scrollComponent.transform).rect.width;
            float scrollElementHeight = eleSize.GetTargetHeight(scrollHeight);
            float scrollElementWidth = eleSize.GetTargetWidth(scrollWidth);
            var anchoredPosition = scrollComponent.content.anchoredPosition;
            var scrollRec = (RectTransform)scrollComponent.transform;
            position = (scrollComponent.vertical) ? scrollRec.rect.height : scrollRec.rect.width;

            if (scrollingDirection == ScrollDirection.BottomToTop || scrollingDirection == ScrollDirection.LeftToight)
            {
                position = -position;
            }

            position = ((scrollComponent.vertical) ? anchoredPosition.y: anchoredPosition.x) - position;

            Vector2 pointA;
            Vector2 pointB;
            if (scrollComponent.vertical)
            {
                pointA = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(eleSize.GetTargetWidth(scrollWidth)/2, position + scrollHeight/2));
                pointB = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(-eleSize.GetTargetWidth(scrollWidth)/2, position + scrollHeight/2));
            }
            else
            {
                pointA = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(eleSize.GetTargetHeight(scrollHeight), position + scrollWidth/2));
                pointB = ((RectTransform)scrollComponent.transform).TransformPoint(new Vector2(-eleSize.GetTargetHeight(scrollHeight), position + scrollWidth/2));
            }
            Debug.DrawLine(pointA, pointB, Color.green, 3.0f);

            return position;
        }
    }
    int GetNumberOfElementsInView
    {
        get
        {
            int number = 0;
            number = (scrollComponent.vertical) ? Mathf.FloorToInt(((RectTransform)scrollComponent.transform).rect.height / (eleSize.GetTargetHeight(((RectTransform)scrollComponent.transform).rect.height) + scrollPadding.top + scrollPadding.bottom)) : Mathf.FloorToInt(((RectTransform)scrollComponent.transform).rect.width/(eleSize.GetTargetHeight(((RectTransform)scrollComponent.transform).rect.width) + scrollPadding.left + scrollPadding.right));
            return number;
        }
    }
    protected virtual string GetUserId(SortingType sortType = SortingType.None)
    {
        return scrollData.userId;
    }
    protected int GetNextImageIndex(SortingType sortType)
    {
        int nextIndex = 0;
        if (sortType == SortingType.FirstToLast)
        {
            nextIndex = (activeElements.Count - 1) + numberOfCulledElements;
        }
        else
        {
            nextIndex = numberOfCulledElements;
        }

        return nextIndex;
    }
#endregion

#region Unity Functions
    void OnDrawGizmos() 
    {
        var scroll = (RectTransform)scrollComponent.transform;
        var targetHeight = eleSize.GetTargetHeight(scroll.rect.height);
        var targetWidth = eleSize.GetTargetWidth(scroll.rect.width);

        var topLeftCorner = new Vector2(-targetWidth/2, targetHeight/2);
        var topRightCorner = new Vector2(targetWidth/2, targetHeight/2);
        var botLeftCorner = new Vector2(-targetWidth/2, -targetHeight/2);
        var botRightCorner = new Vector2(targetWidth/2, -targetHeight/2);

        var worldTLC = scroll.TransformPoint(topLeftCorner);
        var worldTRC = scroll.TransformPoint(topRightCorner);
        var worldBLC = scroll.TransformPoint(botLeftCorner);
        var worldBRC = scroll.TransformPoint(botRightCorner);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(worldTLC, worldTRC);
        Gizmos.DrawLine(worldTRC, worldBRC);
        Gizmos.DrawLine(worldBRC, worldBLC);
        Gizmos.DrawLine(worldBLC, worldTLC);
    }
    void OnEnable() 
    {
        scrollComponent.onValueChanged.AddListener(OnScrollMoved);
    }
#endregion

    void OnScrollMoved(Vector2 scrollPosition)
    {
        if (activeElements.Count == 0)
        {
            return;
        }

        Vector2 deltaPosition;
        if (scrollingDirection == ScrollDirection.TopToBottom || scrollingDirection == ScrollDirection.RightToLeft)
        {
            deltaPosition = -(scrollPosition - lastScrollPosition);
        }
        else
        {
            deltaPosition = scrollPosition - lastScrollPosition;
        }
        // Debug.Log("lastScrollPosition: " + lastScrollPosition);
        lastScrollPosition = scrollPosition;
        // Debug.Log("scrollPosition: " + scrollPosition);

        CheckForSorting(deltaPosition);
    }

    void CheckForSorting(Vector2 deltaPosition)
    {
        if (deltaPosition.y > 0 )
        {
            // Debug.Log("GetFirstElementTopPosition: " + GetScrollTopPosition);
            // Debug.Log("GetFirstElementScrollingPosition - padding.top: " + (GetFirstElementScrollingPosition - padding.top));
            // TODO añadir a l suma de padding y lo otro verificcion de la direccion de scroll.
            if (numberOfCulledElements < GetNumberOfData - activeElements.Count && GetScrollTopPosition >= GetTopSortPosition)
            {
                // Sort first to last
                SortFirstToLast();
            }
        }
        else if (deltaPosition.y < 0 )
        {
            // Debug.Log("GetScrollBottomPosition: " + GetScrollBottomPosition);
            // Debug.Log("GetBotScrollPosition: " + GetBotSortPosition);
            if (numberOfCulledElements > 0 && GetScrollBottomPosition <= GetBotSortPosition)
            {
                // Sort first to last
                SortLastToFirst();
            }
        }
    }
    
    void SortFirstToLast()
    {
        Debug.Log("Sorting first to last");

        var imageToSort = activeElements[0];
        activeElements.Remove(imageToSort);
        activeElements.Add(imageToSort);

        numberOfCulledElements++;
        numberOfCulledElements = Mathf.Clamp(numberOfCulledElements, 0, GetNumberOfData - activeElements.Count);

        imageToSort.transform.SetSiblingIndex(activeElements[activeElements.Count - 2].transform.GetSiblingIndex() + 1);

        var userId = GetUserId();
        var nextImage = GetNextImageIndex(SortingType.FirstToLast);
        UpdateImage(ref imageToSort, userId, nextImage);
        // var imgParameters = new ImageResultParameters()
        // {
        //     ownerId = scrollData.userId,
        //     resultData = scrollData.imgsData.imgCollection[(activeElements.Count - 1) + numberOfCulledElements]
        // };
        // imageToSort.UpdateImageResult(imgParameters);

        // ResetScrollPosition();

        ManageFakeCulledObjectsSize();
    }

    protected virtual void UpdateImage(ref ImageResult image, string userId, int indexData)
    {
        var imgParameters = new ImageResultParameters()
        {
            ownerId = userId,
            resultData = scrollData.imgsData.imgCollection[indexData]
        };
        image.UpdateImageResult(imgParameters);
    }

    void SortLastToFirst()
    {
        Debug.Log("Sorting last to first");

        var imageToSort = activeElements[activeElements.Count - 1];
        activeElements.Remove(imageToSort);
        activeElements.Insert(0, imageToSort);

        numberOfCulledElements--;
        numberOfCulledElements = Mathf.Clamp(numberOfCulledElements, 0, GetNumberOfData - activeElements.Count);

        imageToSort.transform.SetSiblingIndex(activeElements[1].transform.GetSiblingIndex());

        var userId = GetUserId();
        var nextImage = GetNextImageIndex(SortingType.LastToFirst);
        UpdateImage(ref imageToSort, userId, nextImage);
        // var imgParameters = new ImageResultParameters()
        // {
        //     ownerId = scrollData.userId,
        //     resultData = scrollData.imgsData.imgCollection[numberOfCulledElements]
        // };
        
        // imageToSort.UpdateImageResult(imgParameters);

        Debug.Log("numberOfCulledElements: " + numberOfCulledElements);

        // ResetScrollPosition();

        ManageFakeCulledObjectsSize();
    }

    public void Initialize(ScrollData data) 
    {
        Debug.Log("Initializing scroll");

        this.scrollData = data;

        SetupLayoutValues();

        AdjustSrollSize();

        int numberOfElements = GetNumberOfElementsInView;
        SpawnElements(numberOfElements + 2);

        SpawnFakeCulledObjects();
    }

    void SetupLayoutValues()
    {
        float top = 1.0f - eleSize.verticalSizePercent;
        float bot = 1.0f - eleSize.verticalSizePercent;
        scrollPadding.top = (int)top * 100;
        scrollPadding.bottom = (int)bot * 100;


        layoutGroupComponent.padding = scrollPadding;
    }

    void AdjustSrollSize()
    {
        var scroll = (RectTransform)scrollComponent.transform;
        float targetWidth = (scrollComponent.vertical) ? eleSize.GetTargetWidth(scroll.rect.width) : (GetNumberOfData * eleSize.GetTargetWidth(scroll.rect.width)) + scrollPadding.left + scrollPadding.right + ((GetNumberOfData - 1) * layoutGroupComponent.spacing);
        float targetHeight = (scrollComponent.vertical) ? (GetNumberOfData * eleSize.GetTargetHeight(scroll.rect.height)) + scrollPadding.top + scrollPadding.bottom + ((GetNumberOfData - 1) * layoutGroupComponent.spacing) : eleSize.GetTargetHeight(scroll.rect.height);

        scrollComponent.content.sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    void SpawnElements(int numberOfElements)
    {
        foreach (var item in activeElements)
        {
            Destroy(item.gameObject);
        }
        activeElements.Clear();

        var scrollRect = (RectTransform)scrollComponent.transform;
        for (int i = 0; i < numberOfElements && i < GetNumberOfData; i++)
        {
            var img = SpawnImage(i);
            activeElements.Add(img);
            img.SetSizeInPixels(new Vector2(eleSize.GetTargetWidth(scrollRect.rect.width), eleSize.GetTargetHeight(scrollRect.rect.height)));
            img.transform.SetParent(scrollComponent.content.transform, false);
            img.transform.SetSiblingIndex(i);
        }
    }

    protected virtual ImageResult SpawnImage(int dataIndex)
    {
        var img = imagePFB.CreateImageResult(new ImageResultParameters()
        {
            ownerId = scrollData.userId,
            resultData = scrollData.imgsData.imgCollection[dataIndex]
        });

        return img;
    }

    void SpawnFakeCulledObjects()
    {
        if (fakeCulledObjects)
        {
            Destroy(fakeCulledObjects.gameObject);
        }

        fakeCulledObjects = new GameObject("Culled_Objects").AddComponent<LayoutElement>();
        fakeCulledObjects.transform.SetParent(scrollComponent.content.transform, false);
        fakeCulledObjects.transform.SetAsFirstSibling();
        ManageFakeCulledObjectsSize();
    }

    void ManageFakeCulledObjectsSize()
    {
        Vector2 newSize =  Vector2.zero;
        float scrollHeight = ((RectTransform)scrollComponent.transform).rect.height;
        float scrollWidth = ((RectTransform)scrollComponent.transform).rect.width;
        float scrollElementHeight = eleSize.GetTargetHeight(scrollHeight);
        float scrollElementWidth = eleSize.GetTargetWidth(scrollWidth);
        if (scrollComponent.vertical)
        {
            newSize.x += scrollElementWidth;
            newSize.y += (scrollElementHeight * numberOfCulledElements) + scrollPadding.top + (layoutGroupComponent.spacing * (numberOfCulledElements - 1));
        }
        else
        {
            newSize.x += (scrollElementWidth * numberOfCulledElements) + scrollPadding.left + (layoutGroupComponent.spacing * (numberOfCulledElements - 1));
            newSize.y += scrollElementHeight;
        }

        // fakeCulledObjects.ignoreLayout = true;
        fakeCulledObjects.preferredHeight = newSize.y;
        fakeCulledObjects.preferredWidth = newSize.x;
        ((RectTransform)fakeCulledObjects.transform).sizeDelta = newSize;
    }

    void ResetScrollPosition()
    {
        ((RectTransform)scrollComponent.content.transform).anchoredPosition = Vector3.zero;
    }
}
