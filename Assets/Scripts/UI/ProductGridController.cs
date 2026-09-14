using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductGridController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private ProductManager productManager;

    [SerializeField]
    private ThumbnailCacheService thumbnailCacheService;


    [Header("Grid")]
    [SerializeField]
    private ProductCardView productCardPrefab;

    [SerializeField]
    private ProductDetailController productDetailController;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private RectTransform viewport;

    [SerializeField]
    private GridLayoutGroup gridLayout;


    [Header("Virtualization")]
    [Tooltip(
        "Extra rows kept just outside the viewport for smoother scrolling."
    )]
    [SerializeField, Min(0)]
    private int bufferRows = 1;


    [Header("Catalogue Transition")]
    [SerializeField]
    private CanvasGroup catalogueGroup;

    [SerializeField]
    private float catalogueFadeDuration = 0.20f;


    [Header("Empty State")]
    [SerializeField]
    private EmptyStateView emptyStateView;

    [Header("Catalogue Header")]
    [SerializeField] private TextMeshProUGUI catalogueTitle;

    private readonly List<ProductCardView>
        cardPool =
            new List<ProductCardView>();


    private Vector2 cellSize;
    private Vector2 spacing;
    private RectOffset padding;

    private int columnCount = 1;
    private int totalRows;
    private int poolRows;
    private int firstPooledRow = -1;

    private bool initialized;
    private Vector2 lastViewportSize;

    private Coroutine catalogueFadeRoutine;


    private void OnEnable()
    {
        if (productManager != null)
        {
            productManager.ProductsChanged +=
                RefreshProducts;
        }

        if (scrollRect != null)
        {
            scrollRect.onValueChanged
                .AddListener(
                    OnScrollChanged
                );
        }
    }


    private void OnDisable()
    {
        if (productManager != null)
        {
            productManager.ProductsChanged -=
                RefreshProducts;
        }

        if (scrollRect != null)
        {
            scrollRect.onValueChanged
                .RemoveListener(
                    OnScrollChanged
                );
        }
    }


    private void Start()
    {
        InitializeVirtualGrid();


        // Keep your previous startup behaviour.
        // This prevents "No products found"
        // flashing before JSON finishes loading.
        if (productManager
                .CurrentProducts
                .Count > 0)
        {
            RefreshProducts();
        }
        else
        {
            UpdateVirtualContentHeight();
        }
    }


    private void Update()
    {
        if (!initialized)
            return;


        Vector2 currentSize =
            viewport.rect.size;


        // Rebuild for resolution/orientation changes.
        if (
            Mathf.Abs(
                currentSize.x -
                lastViewportSize.x
            ) > 1f ||
            Mathf.Abs(
                currentSize.y -
                lastViewportSize.y
            ) > 1f)
        {
            RebuildForViewportSize();
        }
    }


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void InitializeVirtualGrid()
    {
        if (initialized)
            return;


        if (
            productManager == null ||
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


        // Read GridLayoutGroup as configuration.
        cellSize =
            gridLayout.cellSize;

        spacing =
            gridLayout.spacing;


        // Copy padding.
        padding =
            new RectOffset(
                gridLayout.padding.left,
                gridLayout.padding.right,
                gridLayout.padding.top,
                gridLayout.padding.bottom
            );


        // Virtual grid manually positions cards.
        gridLayout.enabled = false;


        ContentSizeFitter fitter =
            content.GetComponent<
                ContentSizeFitter
            >();

        if (fitter != null)
        {
            fitter.enabled = false;
        }


        initialized = true;

        RebuildForViewportSize();
    }


    private void RebuildForViewportSize()
    {
        if (!initialized)
            return;


        Canvas.ForceUpdateCanvases();

        lastViewportSize =
            viewport.rect.size;


        CalculateColumnCount();
        CalculatePoolRows();
        EnsurePoolSize();
        UpdateVirtualContentHeight();


        firstPooledRow = -1;

        RefreshVisibleCards(true);
    }


    // =========================================================
    // GRID CALCULATIONS
    // =========================================================

    private void CalculateColumnCount()
    {
        // Your current layout uses
        // FixedColumnCount = 2.
        if (
            gridLayout.constraint ==
            GridLayoutGroup.Constraint
                .FixedColumnCount)
        {
            columnCount =
                Mathf.Max(
                    1,
                    gridLayout
                        .constraintCount
                );

            return;
        }


        float availableWidth =
            viewport.rect.width -
            padding.left -
            padding.right;


        float cellStrideX =
            cellSize.x +
            spacing.x;


        if (cellStrideX <= 0f)
        {
            columnCount = 1;
            return;
        }


        columnCount =
            Mathf.Max(
                1,
                Mathf.FloorToInt(
                    (availableWidth +
                     spacing.x) /
                    cellStrideX
                )
            );
    }


    private void CalculatePoolRows()
    {
        float rowStride =
            cellSize.y +
            spacing.y;


        if (rowStride <= 0f)
        {
            poolRows = 1;
            return;
        }


        int visibleRows =
            Mathf.CeilToInt(
                viewport.rect.height /
                rowStride
            ) + 1;


        poolRows =
            Mathf.Max(
                1,
                visibleRows +
                (bufferRows * 2)
            );
    }


    private void EnsurePoolSize()
    {
        int requiredCardCount =
            poolRows *
            columnCount;


        while (
            cardPool.Count <
            requiredCardCount)
        {
            ProductCardView card =
                Instantiate(
                    productCardPrefab,
                    content
                );


            RectTransform cardRect =
                card.transform
                    as RectTransform;


            cardRect.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            cardRect.anchorMax =
                new Vector2(
                    0f,
                    1f
                );

            cardRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            cardRect.sizeDelta =
                cellSize;


            card.gameObject
                .SetActive(false);


            cardPool.Add(card);
        }


        // Disable unused pooled cards.
        for (
            int i =
                requiredCardCount;
            i < cardPool.Count;
            i++)
        {
            cardPool[i]
                .ClearView();

            cardPool[i]
                .gameObject
                .SetActive(false);
        }
    }


    // =========================================================
    // PRODUCT REFRESH
    // =========================================================

    public void RefreshProducts()
    {
        if (!initialized)
        {
            InitializeVirtualGrid();
        }


        if (!initialized)
            return;


        int productCount =
            productManager
                .CurrentProducts
                .Count;


        Debug.Log(
            "[GRID] Virtual refresh with " +
            productCount +
            " products. Pool objects: " +
            cardPool.Count
        );


        // -----------------------------
        // Empty result state
        // -----------------------------

        if (emptyStateView != null)
        {
            emptyStateView.SetVisible(
                productCount == 0
            );
        }


        // -----------------------------
        // Start catalogue transition
        // -----------------------------

        if (catalogueGroup != null)
        {
            catalogueGroup.alpha =
                0f;

            catalogueGroup.interactable =
                false;
        }


        // Return catalogue to top
        // after new filters are applied.
        Vector2 contentPosition =
            content.anchoredPosition;

        contentPosition.y = 0f;

        content.anchoredPosition =
            contentPosition;


        UpdateVirtualContentHeight();


        firstPooledRow = -1;

        RefreshVisibleCards(true);


        PlayCatalogueFadeIn();
        bool hasFilter =
    productManager.AppliedFilter.selectedCategories.Count > 0 ||
    productManager.AppliedFilter.selectedSubcategories.Count > 0 ||
    productManager.AppliedFilter.selectedProductIds.Count > 0;

if (catalogueTitle != null)
{
    catalogueTitle.text =
        hasFilter ? "Results" : "Trending Now";
}
    }


    // =========================================================
    // CATALOGUE FADE
    // =========================================================

    private void PlayCatalogueFadeIn()
    {
        if (catalogueGroup == null)
            return;


        if (catalogueFadeRoutine != null)
        {
            StopCoroutine(
                catalogueFadeRoutine
            );
        }


        catalogueFadeRoutine =
            StartCoroutine(
                FadeCatalogueIn()
            );
    }


    private IEnumerator FadeCatalogueIn()
    {
        float elapsed = 0f;


        catalogueGroup.alpha =
            0f;

        catalogueGroup.interactable =
            false;


        while (
            elapsed <
            catalogueFadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    catalogueFadeDuration
                );


            // Smoothstep.
            t =
                t * t *
                (3f - (2f * t));


            catalogueGroup.alpha =
                t;


            yield return null;
        }


        catalogueGroup.alpha =
            1f;

        catalogueGroup.interactable =
            true;


        catalogueFadeRoutine =
            null;
    }


    // =========================================================
    // VIRTUAL CONTENT
    // =========================================================

    private void UpdateVirtualContentHeight()
    {
        if (!initialized)
            return;


        int productCount =
            productManager
                .CurrentProducts
                .Count;


        totalRows =
            productCount == 0
                ? 0
                : Mathf.CeilToInt(
                    productCount /
                    (float)columnCount
                );


        float height =
            padding.top +
            padding.bottom;


        if (totalRows > 0)
        {
            height +=
                (totalRows *
                 cellSize.y) +
                ((totalRows - 1) *
                 spacing.y);
        }


        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(
                0f,
                height
            )
        );
    }


    private void OnScrollChanged(
        Vector2 _)
    {
        RefreshVisibleCards(false);
    }


    private void RefreshVisibleCards(
        bool force)
    {
        if (
            !initialized ||
            cardPool.Count == 0)
        {
            return;
        }


        int productCount =
            productManager
                .CurrentProducts
                .Count;


        if (productCount == 0)
        {
            HideAllCards();
            return;
        }


        float rowStride =
            cellSize.y +
            spacing.y;


        if (rowStride <= 0f)
            return;


        float scrollY =
            Mathf.Max(
                0f,
                content
                    .anchoredPosition
                    .y
            );


        int visibleFirstRow =
            Mathf.FloorToInt(
                scrollY /
                rowStride
            );


        int newFirstPooledRow =
            Mathf.Max(
                0,
                visibleFirstRow -
                bufferRows
            );


        if (
            !force &&
            newFirstPooledRow ==
            firstPooledRow)
        {
            return;
        }


        firstPooledRow =
            newFirstPooledRow;


        for (
            int poolIndex = 0;
            poolIndex <
            cardPool.Count;
            poolIndex++)
        {
            int localRow =
                poolIndex /
                columnCount;


            int column =
                poolIndex %
                columnCount;


            int actualRow =
                firstPooledRow +
                localRow;


            int productIndex =
                (actualRow *
                 columnCount) +
                column;


            ProductCardView card =
                cardPool[
                    poolIndex
                ];


            if (
                actualRow >= totalRows ||
                productIndex >=
                productCount)
            {
                card.ClearView();

                card.gameObject
                    .SetActive(false);

                continue;
            }


            PositionCard(
                card.transform
                    as RectTransform,
                actualRow,
                column
            );


            if (!card
                    .gameObject
                    .activeSelf)
            {
                card.gameObject
                    .SetActive(true);
            }


            card.SetData(
                productManager
                    .CurrentProducts[
                        productIndex
                    ],
                thumbnailCacheService,
                productDetailController
            );
        }
    }


    private void PositionCard(
        RectTransform cardRect,
        int row,
        int column)
    {
        float gridWidth =
            (columnCount *
             cellSize.x) +
            ((columnCount - 1) *
             spacing.x);


        float availableWidth =
            content.rect.width -
            padding.left -
            padding.right;


        float centeredOffset =
            Mathf.Max(
                0f,
                (availableWidth -
                 gridWidth) *
                0.5f
            );


        float startX =
            padding.left +
            centeredOffset;


        float x =
            startX +
            (column *
             (cellSize.x +
              spacing.x)) +
            (cellSize.x *
             0.5f);


        float y =
            -padding.top -
            (row *
             (cellSize.y +
              spacing.y)) -
            (cellSize.y *
             0.5f);


        cardRect.anchoredPosition =
            new Vector2(
                x,
                y
            );
    }


    private void HideAllCards()
    {
        foreach (
            ProductCardView card
            in cardPool)
        {
            card.ClearView();

            card.gameObject
                .SetActive(false);
        }
    }
}