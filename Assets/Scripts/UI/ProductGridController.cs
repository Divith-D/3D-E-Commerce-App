using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductGridController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ProductManager productManager;
    [SerializeField] private ThumbnailCacheService thumbnailCacheService;

    [Header("Grid")]
    [SerializeField] private ProductCardView productCardPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private GridLayoutGroup gridLayout;

    [Header("Virtualization")]
    [Tooltip("Extra rows kept just outside the viewport for smoother scrolling.")]
    [SerializeField, Min(0)] private int bufferRows = 1;

    private readonly List<ProductCardView> cardPool = new List<ProductCardView>();

    private Vector2 cellSize;
    private Vector2 spacing;
    private RectOffset padding;

    private int columnCount = 1;
    private int totalRows;
    private int poolRows;
    private int firstPooledRow = -1;

    private bool initialized;
    private Vector2 lastViewportSize;


    private void OnEnable()
    {
        if (productManager != null)
            productManager.ProductsChanged += RefreshProducts;

        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }


    private void OnDisable()
    {
        if (productManager != null)
            productManager.ProductsChanged -= RefreshProducts;

        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }


    private void Start()
    {
        InitializeVirtualGrid();

        // Handles the case where products were already loaded.
        if (productManager.CurrentProducts.Count > 0)
            RefreshProducts();
        else
            UpdateVirtualContentHeight();
    }


    private void Update()
    {
        if (!initialized)
            return;

        Vector2 currentSize = viewport.rect.size;

        // Rebuild when orientation / viewport size changes.
        if (Mathf.Abs(currentSize.x - lastViewportSize.x) > 1f ||
            Mathf.Abs(currentSize.y - lastViewportSize.y) > 1f)
        {
            RebuildForViewportSize();
        }
    }


    private void InitializeVirtualGrid()
    {
        if (initialized)
            return;

        if (productManager == null ||
            thumbnailCacheService == null ||
            productCardPrefab == null ||
            scrollRect == null ||
            content == null ||
            viewport == null ||
            gridLayout == null)
        {
            Debug.LogError(
                "[GRID] ProductGridController is missing one or more Inspector references."
            );
            return;
        }

        Canvas.ForceUpdateCanvases();

        // Read the existing GridLayoutGroup only as layout configuration.
        cellSize = gridLayout.cellSize;
        spacing = gridLayout.spacing;

        // Copy padding because RectOffset belongs to the GridLayoutGroup.
        padding = new RectOffset(
            gridLayout.padding.left,
            gridLayout.padding.right,
            gridLayout.padding.top,
            gridLayout.padding.bottom
        );

        // GridLayoutGroup cannot remain enabled because virtualization manually
        // positions recycled cards at arbitrary rows.
        gridLayout.enabled = false;

        // ContentSizeFitter would fight our manually calculated virtual height.
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;

        // Keep the Content RectTransform exactly as configured in the Inspector.
        // The user's layout is already top-stretched with a top pivot.

        initialized = true;

        RebuildForViewportSize();
    }


    private void RebuildForViewportSize()
    {
        if (!initialized)
            return;

        Canvas.ForceUpdateCanvases();

        lastViewportSize = viewport.rect.size;

        CalculateColumnCount();
        CalculatePoolRows();
        EnsurePoolSize();
        UpdateVirtualContentHeight();

        firstPooledRow = -1;
        RefreshVisibleCards(true);
    }


    private void CalculateColumnCount()
    {
        // Preserve the GridLayoutGroup's intended flow.
        // In the current UI this is FixedColumnCount = 2.
        if (gridLayout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columnCount = Mathf.Max(1, gridLayout.constraintCount);
            return;
        }

        float availableWidth =
            viewport.rect.width -
            padding.left -
            padding.right;

        float cellStrideX = cellSize.x + spacing.x;

        if (cellStrideX <= 0f)
        {
            columnCount = 1;
            return;
        }

        columnCount = Mathf.Max(
            1,
            Mathf.FloorToInt(
                (availableWidth + spacing.x) / cellStrideX
            )
        );
    }


    private void CalculatePoolRows()
    {
        float rowStride = cellSize.y + spacing.y;

        if (rowStride <= 0f)
        {
            poolRows = 1;
            return;
        }

        int visibleRows =
            Mathf.CeilToInt(viewport.rect.height / rowStride) + 1;

        // Keep a small recycled buffer above and below the visible viewport.
        poolRows = Mathf.Max(
            1,
            visibleRows + (bufferRows * 2)
        );
    }


    private void EnsurePoolSize()
    {
        int requiredCardCount = poolRows * columnCount;

        while (cardPool.Count < requiredCardCount)
        {
            ProductCardView card =
                Instantiate(productCardPrefab, content);

            RectTransform cardRect =
                card.transform as RectTransform;

            cardRect.anchorMin = new Vector2(0f, 1f);
            cardRect.anchorMax = new Vector2(0f, 1f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = cellSize;

            card.gameObject.SetActive(false);
            cardPool.Add(card);
        }

        // If orientation changes and fewer cards are needed, keep the objects
        // in the pool but disable the unused tail. No destroy/re-instantiation.
        for (int i = requiredCardCount; i < cardPool.Count; i++)
        {
            cardPool[i].ClearView();
            cardPool[i].gameObject.SetActive(false);
        }
    }


    public void RefreshProducts()
    {
        if (!initialized)
            InitializeVirtualGrid();

        if (!initialized)
            return;

        Debug.Log(
            "[GRID] Virtual refresh with " +
            productManager.CurrentProducts.Count +
            " products. Pool objects: " +
            cardPool.Count
        );

        // After applying a new filter, return catalogue to the top.
        Vector2 contentPosition = content.anchoredPosition;
        contentPosition.y = 0f;
        content.anchoredPosition = contentPosition;

        UpdateVirtualContentHeight();

        firstPooledRow = -1;
        RefreshVisibleCards(true);
    }


    private void UpdateVirtualContentHeight()
    {
        if (!initialized)
            return;

        int productCount = productManager.CurrentProducts.Count;

        totalRows =
            productCount == 0
                ? 0
                : Mathf.CeilToInt(
                    productCount / (float)columnCount
                );

        float height =
            padding.top +
            padding.bottom;

        if (totalRows > 0)
        {
            height +=
                (totalRows * cellSize.y) +
                ((totalRows - 1) * spacing.y);
        }

        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(0f, height)
        );
    }


    private void OnScrollChanged(Vector2 _)
    {
        RefreshVisibleCards(false);
    }


    private void RefreshVisibleCards(bool force)
    {
        if (!initialized || cardPool.Count == 0)
            return;

        int productCount =
            productManager.CurrentProducts.Count;

        if (productCount == 0)
        {
            HideAllCards();
            return;
        }

        float rowStride = cellSize.y + spacing.y;

        if (rowStride <= 0f)
            return;

        float scrollY =
            Mathf.Max(0f, content.anchoredPosition.y);

        int visibleFirstRow =
            Mathf.FloorToInt(scrollY / rowStride);

        int newFirstPooledRow =
            Mathf.Max(0, visibleFirstRow - bufferRows);

        if (!force &&
            newFirstPooledRow == firstPooledRow)
        {
            return;
        }

        firstPooledRow = newFirstPooledRow;

        for (int poolIndex = 0;
             poolIndex < cardPool.Count;
             poolIndex++)
        {
            int localRow =
                poolIndex / columnCount;

            int column =
                poolIndex % columnCount;

            int actualRow =
                firstPooledRow + localRow;

            int productIndex =
                (actualRow * columnCount) + column;

            ProductCardView card =
                cardPool[poolIndex];

            if (actualRow >= totalRows ||
                productIndex >= productCount)
            {
                card.ClearView();
                card.gameObject.SetActive(false);
                continue;
            }

            PositionCard(
                card.transform as RectTransform,
                actualRow,
                column
            );

            if (!card.gameObject.activeSelf)
                card.gameObject.SetActive(true);

            card.SetData(
                productManager.CurrentProducts[productIndex],
                thumbnailCacheService
            );
        }
    }


    private void PositionCard(
        RectTransform cardRect,
        int row,
        int column)
    {
        // Reproduce GridLayoutGroup -> Child Alignment = Upper Center.
        float gridWidth =
            (columnCount * cellSize.x) +
            ((columnCount - 1) * spacing.x);

        float availableWidth =
            content.rect.width -
            padding.left -
            padding.right;

        float centeredOffset =
            Mathf.Max(0f, (availableWidth - gridWidth) * 0.5f);

        float startX =
            padding.left + centeredOffset;

        float x =
            startX +
            (column * (cellSize.x + spacing.x)) +
            (cellSize.x * 0.5f);

        float y =
            -padding.top -
            (row * (cellSize.y + spacing.y)) -
            (cellSize.y * 0.5f);

        cardRect.anchoredPosition =
            new Vector2(x, y);
    }


    private void HideAllCards()
    {
        foreach (ProductCardView card in cardPool)
        {
            card.ClearView();
            card.gameObject.SetActive(false);
        }
    }
}
