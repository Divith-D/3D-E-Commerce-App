using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterPanelController : MonoBehaviour
{
    // =========================================================
    // CORE
    // =========================================================

    [Header("Core")]
    [SerializeField] private ProductManager productManager;


    // =========================================================
    // CANVAS GROUPS
    // =========================================================

    [Header("Sections")]
    [SerializeField] private CanvasGroup filterPanelGroup;
    [SerializeField] private CanvasGroup subcategorySectionGroup;
    [SerializeField] private CanvasGroup itemsSectionGroup;


    // =========================================================
    // MAIN FILTER BUTTON
    // =========================================================

    [Header("Filter Button")]
    [SerializeField] private Button filterButton;


    // =========================================================
    // CATEGORY BUTTONS
    // =========================================================

    [Header("Category Buttons")]
    [SerializeField] private Button watchesButton;
    [SerializeField] private Button clothesButton;
    [SerializeField] private Button jewelleryButton;


    // =========================================================
    // SUBCATEGORY BUTTONS
    // =========================================================

    [Header("Subcategory Buttons")]
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;
    [SerializeField] private Button kidsBoyButton;
    [SerializeField] private Button kidsGirlButton;


    // =========================================================
    // ACTION BUTTONS
    // =========================================================

    [Header("Action Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;


    // =========================================================
    // FILTER ITEM LIST
    // =========================================================

    [Header("Filter Item List")]
    [SerializeField] private FilterItemView filterItemPrefab;
    [SerializeField] private Transform filterItemParent;
    [SerializeField] private ThumbnailCacheService thumbnailCacheService;


    // =========================================================
    // CHIP VISUALS
    // =========================================================

    [Header("Chip Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;


    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Panel Animation")]
    [SerializeField] private float fadeDuration = 0.18f;


    private FilterState draftFilter;


    private readonly Dictionary<CanvasGroup, Coroutine> fadeRoutines =
        new Dictionary<CanvasGroup, Coroutine>();


    [Header("Filter Item Animation")]
[SerializeField] private float itemFadeDuration = 0.16f;
[SerializeField] private float itemStagger = 0.035f;
[SerializeField] private float itemStartScale = 0.97f;
    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Main Filter button
        filterButton.onClick.AddListener(OpenPanel);


        // Category buttons
        watchesButton.onClick.AddListener(
            () => ToggleCategory("Watches")
        );

        clothesButton.onClick.AddListener(
            () => ToggleCategory("Clothes")
        );

        jewelleryButton.onClick.AddListener(
            () => ToggleCategory("Jewellery")
        );


        // Subcategory buttons
        maleButton.onClick.AddListener(
            () => ToggleSubcategory("Male")
        );

        femaleButton.onClick.AddListener(
            () => ToggleSubcategory("Female")
        );

        kidsBoyButton.onClick.AddListener(
            () => ToggleSubcategory("Kids-Boy")
        );

        kidsGirlButton.onClick.AddListener(
            () => ToggleSubcategory("Kids-Girl")
        );


        // Action buttons
        applyButton.onClick.AddListener(ApplyFilter);
        resetButton.onClick.AddListener(ResetFilter);
        closeButton.onClick.AddListener(ClosePanel);


        // Initial state
        SetCanvasGroupImmediate(
            filterPanelGroup,
            false
        );

        SetCanvasGroupImmediate(
            subcategorySectionGroup,
            false
        );

        SetCanvasGroupImmediate(
            itemsSectionGroup,
            false
        );
    }


    // =========================================================
    // OPEN PANEL
    // =========================================================

    public void OpenPanel()
    {
        draftFilter =
            productManager.AppliedFilter.Clone();


        RefreshFilterUI();
        RefreshItemList();


        SetCanvasGroupVisible(
            filterPanelGroup,
            true
        );
    }


    // =========================================================
    // CATEGORY
    // =========================================================

    private void ToggleCategory(string category)
    {
        EnsureDraftExists();

        ToggleValue(
            draftFilter.selectedCategories,
            category
        );

        // IMPORTANT:
        // Do NOT remove selectedProductIds here.
        // Checked products must remain remembered
        // while the user browses other filters.

        RefreshFilterUI();
        RefreshItemList();
    }


    // =========================================================
    // SUBCATEGORY
    // =========================================================

    private void ToggleSubcategory(string subcategory)
    {
        EnsureDraftExists();

        ToggleValue(
            draftFilter.selectedSubcategories,
            subcategory
        );

        // IMPORTANT:
        // Do NOT remove selectedProductIds here either.

        RefreshFilterUI();
        RefreshItemList();
    }

    // =========================================================
    // APPLY
    // =========================================================

    public void ApplyFilter()
    {
        EnsureDraftExists();

        FilterState effectiveFilter =
            BuildEffectiveFilter();

        productManager.ApplyFilter(
            effectiveFilter
        );

        SetCanvasGroupVisible(
            filterPanelGroup,
            false
        );
    }
    private FilterState BuildEffectiveFilter()
    {
        FilterState effectiveFilter =
            draftFilter.Clone();


        // If the user has not currently reached the
        // item-selection level, remembered item checks
        // should not restrict the catalogue.
        if (effectiveFilter.selectedCategories.Count == 0 ||
            effectiveFilter.selectedSubcategories.Count == 0)
        {
            effectiveFilter.selectedProductIds.Clear();

            return effectiveFilter;
        }


        // Keep remembered checkmarks in draftFilter,
        // but only APPLY checked items that belong to
        // the currently active category/subcategory.
        for (int i =
                 effectiveFilter.selectedProductIds.Count - 1;
             i >= 0;
             i--)
        {
            string productId =
                effectiveFilter.selectedProductIds[i];


            ProductData product =
                productManager.allProducts.Find(
                    p => p.productId == productId
                );


            if (product == null)
            {
                effectiveFilter
                    .selectedProductIds
                    .RemoveAt(i);

                continue;
            }


            bool categoryActive =
                effectiveFilter
                    .selectedCategories
                    .Contains(product.category);


            bool subcategoryActive =
                effectiveFilter
                    .selectedSubcategories
                    .Contains(product.subcategory);


            if (!categoryActive ||
                !subcategoryActive)
            {
                effectiveFilter
                    .selectedProductIds
                    .RemoveAt(i);
            }
        }


        return effectiveFilter;
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetFilter()
    {
        EnsureDraftExists();


        draftFilter.Clear();


        productManager.ResetFilter();


        RefreshFilterUI();
        RefreshItemList();
    }


    // =========================================================
    // CLOSE
    // =========================================================

    public void ClosePanel()
    {
        // Discard unapplied temporary selections
        draftFilter = null;


        SetCanvasGroupVisible(
            filterPanelGroup,
            false
        );
    }


    // =========================================================
    // UI STATE
    // =========================================================

    private void RefreshFilterUI()
    {
        if (draftFilter == null)
            return;


        bool categorySelected =
            draftFilter.selectedCategories.Count > 0;


        bool subcategorySelected =
            draftFilter.selectedSubcategories.Count > 0;


        // Category selected -> show subcategories
        SetCanvasGroupVisible(
            subcategorySectionGroup,
            categorySelected
        );


        // Category + Subcategory selected -> show items
        SetCanvasGroupVisible(
            itemsSectionGroup,
            categorySelected &&
            subcategorySelected
        );


        // -------------------------
        // Category chip feedback
        // -------------------------

        SetButtonState(
            watchesButton,
            draftFilter.selectedCategories.Contains(
                "Watches"
            )
        );


        SetButtonState(
            clothesButton,
            draftFilter.selectedCategories.Contains(
                "Clothes"
            )
        );


        SetButtonState(
            jewelleryButton,
            draftFilter.selectedCategories.Contains(
                "Jewellery"
            )
        );


        // -------------------------
        // Subcategory chip feedback
        // -------------------------

        SetButtonState(
            maleButton,
            draftFilter.selectedSubcategories.Contains(
                "Male"
            )
        );


        SetButtonState(
            femaleButton,
            draftFilter.selectedSubcategories.Contains(
                "Female"
            )
        );


        SetButtonState(
            kidsBoyButton,
            draftFilter.selectedSubcategories.Contains(
                "Kids-Boy"
            )
        );


        SetButtonState(
            kidsGirlButton,
            draftFilter.selectedSubcategories.Contains(
                "Kids-Girl"
            )
        );
    }


    // =========================================================
    // CHIP VISUAL
    // =========================================================

    private void SetButtonState(
        Button button,
        bool selected)
    {
        if (button == null ||
            button.image == null)
        {
            return;
        }


        button.image.color =
            selected
                ? selectedColor
                : normalColor;
    }


    // =========================================================
    // CANVAS GROUP FADE
    // =========================================================

    private void SetCanvasGroupVisible(
        CanvasGroup group,
        bool visible)
    {
        if (group == null)
            return;


        if (
            fadeRoutines.TryGetValue(
                group,
                out Coroutine runningRoutine
            ))
        {
            if (runningRoutine != null)
            {
                StopCoroutine(
                    runningRoutine
                );
            }
        }


        float targetAlpha =
            visible ? 1f : 0f;


        // Already at requested state
        if (
            Mathf.Approximately(
                group.alpha,
                targetAlpha
            ))
        {
            group.alpha =
                targetAlpha;

            group.interactable =
                visible;

            group.blocksRaycasts =
                visible;

            fadeRoutines[group] =
                null;

            return;
        }


        // Input behaviour during fade
        if (visible)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }


        fadeRoutines[group] =
            StartCoroutine(
                FadeCanvasGroup(
                    group,
                    visible
                )
            );
    }


    private IEnumerator FadeCanvasGroup(
        CanvasGroup group,
        bool visible)
    {
        float startAlpha =
            group.alpha;


        float targetAlpha =
            visible ? 1f : 0f;


        float elapsed = 0f;


        while (elapsed < fadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );


            // Smoothstep
            t =
                t * t *
                (3f - (2f * t));


            group.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );


            yield return null;
        }


        group.alpha =
            targetAlpha;


        group.interactable =
            visible;


        group.blocksRaycasts =
            visible;


        fadeRoutines[group] =
            null;
    }


    // =========================================================
    // IMMEDIATE VISIBILITY
    // =========================================================

    private void SetCanvasGroupImmediate(
        CanvasGroup group,
        bool visible)
    {
        if (group == null)
            return;


        group.alpha =
            visible ? 1f : 0f;


        group.interactable =
            visible;


        group.blocksRaycasts =
            visible;
    }


    // =========================================================
    // FILTER HELPERS
    // =========================================================

    private void ToggleValue(
        List<string> list,
        string value)
    {
        if (list.Contains(value))
        {
            list.Remove(value);
        }
        else
        {
            list.Add(value);
        }
    }


    private void EnsureDraftExists()
    {
        if (draftFilter != null)
            return;


        draftFilter =
            productManager
                .AppliedFilter
                .Clone();
    }


    // =========================================================
    // ITEM LIST
    // =========================================================

    private void RefreshItemList()
{
    if (filterItemParent == null)
        return;


    // -----------------------------------------
    // REMOVE OLD RESULTS IMMEDIATELY
    // -----------------------------------------

    for (
        int i = filterItemParent.childCount - 1;
        i >= 0;
        i--)
    {
        GameObject oldItem =
            filterItemParent
                .GetChild(i)
                .gameObject;

        // Hide immediately.
        // Destroy happens at end of frame.
        oldItem.SetActive(false);

        Destroy(oldItem);
    }


    // -----------------------------------------
    // NOTHING TO SHOW YET
    // -----------------------------------------

    if (
        draftFilter == null ||
        draftFilter.selectedCategories.Count == 0 ||
        draftFilter.selectedSubcategories.Count == 0)
    {
        return;
    }


    int visibleIndex = 0;


    // -----------------------------------------
    // CREATE FILTERED RESULTS
    // -----------------------------------------

    foreach (
        ProductData product
        in productManager.allProducts)
    {
        bool categoryMatch =
            draftFilter
                .selectedCategories
                .Contains(
                    product.category
                );


        bool subcategoryMatch =
            draftFilter
                .selectedSubcategories
                .Contains(
                    product.subcategory
                );


        if (
            !categoryMatch ||
            !subcategoryMatch)
        {
            continue;
        }


        bool selected =
            draftFilter
                .selectedProductIds
                .Contains(
                    product.productId
                );


        FilterItemView item =
            Instantiate(
                filterItemPrefab,
                filterItemParent
            );


        item.Setup(
            product,
            thumbnailCacheService,
            selected,
            OnItemSelectionChanged
        );


        // -----------------------------------------
        // SMOOTH LOAD
        // -----------------------------------------

        StartCoroutine(
            AnimateFilterItemIn(
                item.transform,
                visibleIndex
            )
        );


        visibleIndex++;
    }
}
private IEnumerator AnimateFilterItemIn(
    Transform itemTransform,
    int index)
{
    if (itemTransform == null)
        yield break;


    CanvasGroup group =
        itemTransform.GetComponent<CanvasGroup>();


    if (group == null)
    {
        group =
            itemTransform.gameObject
                .AddComponent<CanvasGroup>();
    }


    Vector3 normalScale =
        itemTransform.localScale;


    Vector3 startScale =
        normalScale * itemStartScale;


    group.alpha = 0f;

    itemTransform.localScale =
        startScale;


    // Don't keep increasing the delay forever
    // if reviewer loads a huge JSON.
    float delay =
        Mathf.Min(index, 8) *
        itemStagger;


    if (delay > 0f)
    {
        yield return new WaitForSecondsRealtime(
            delay
        );
    }


    float elapsed = 0f;


    while (elapsed < itemFadeDuration)
    {
        if (itemTransform == null)
            yield break;


        elapsed +=
            Time.unscaledDeltaTime;


        float t =
            Mathf.Clamp01(
                elapsed /
                itemFadeDuration
            );


        float smoothT =
            Mathf.SmoothStep(
                0f,
                1f,
                t
            );


        group.alpha =
            smoothT;


        itemTransform.localScale =
            Vector3.Lerp(
                startScale,
                normalScale,
                smoothT
            );


        yield return null;
    }


    group.alpha = 1f;

    itemTransform.localScale =
        normalScale;
}


    // =========================================================
    // ITEM CHECKBOX CALLBACK
    // =========================================================

    private void OnItemSelectionChanged(
        string productId,
        bool selected)
    {
        EnsureDraftExists();


        if (selected)
        {
            if (
                !draftFilter
                    .selectedProductIds
                    .Contains(productId))
            {
                draftFilter
                    .selectedProductIds
                    .Add(productId);
            }
        }
        else
        {
            draftFilter
                .selectedProductIds
                .Remove(productId);
        }
    }
}