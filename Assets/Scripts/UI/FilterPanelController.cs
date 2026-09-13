using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterPanelController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private ProductManager productManager;

    [Header("Sections")]
    [SerializeField] private CanvasGroup filterPanelGroup;
    [SerializeField] private CanvasGroup subcategorySectionGroup;
    [SerializeField] private CanvasGroup itemsSectionGroup;

    [Header("Action Buttons")]
    [SerializeField] private Button openFilterButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button topCloseButton;

    [Header("Category Buttons")]
    [SerializeField] private Button watchesButton;
    [SerializeField] private Button clothesButton;
    [SerializeField] private Button jewelleryButton;

    [Header("Subcategory Buttons")]
    [SerializeField] private Button maleButton;
    [SerializeField] private Button femaleButton;
    [SerializeField] private Button kidsBoyButton;
    [SerializeField] private Button kidsGirlButton;

    [Header("Filter Item List")]
    [SerializeField] private FilterItemView filterItemPrefab;
    [SerializeField] private Transform filterItemParent;
    [SerializeField] private ThumbnailCacheService thumbnailCacheService;

    [Header("Chip Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.gray;

    private FilterState draftFilter;


    private void Start()
    {
        // Main filter action buttons are wired here in code.
        // No Button.OnClick setup is required in the Unity Inspector.
        if (openFilterButton != null)
            openFilterButton.onClick.AddListener(OpenPanel);

        if (applyButton != null)
            applyButton.onClick.AddListener(ApplyFilter);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetFilter);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (topCloseButton != null)
            topCloseButton.onClick.AddListener(ClosePanel);

        watchesButton.onClick.AddListener(
            () => ToggleCategory("Watches"));

        clothesButton.onClick.AddListener(
            () => ToggleCategory("Clothes"));

        jewelleryButton.onClick.AddListener(
            () => ToggleCategory("Jewellery"));


        maleButton.onClick.AddListener(
            () => ToggleSubcategory("Male"));

        femaleButton.onClick.AddListener(
            () => ToggleSubcategory("Female"));

        kidsBoyButton.onClick.AddListener(
            () => ToggleSubcategory("Kids-Boy"));

        kidsGirlButton.onClick.AddListener(
            () => ToggleSubcategory("Kids-Girl"));


        SetCanvasGroupVisible(
            filterPanelGroup,
            false
        );

        SetCanvasGroupVisible(
            subcategorySectionGroup,
            false
        );

        SetCanvasGroupVisible(
            itemsSectionGroup,
            false
        );
    }


    public void OpenPanel()
    {
        draftFilter =
            productManager.AppliedFilter.Clone();

        SetCanvasGroupVisible(
            filterPanelGroup,
            true
        );

        RefreshFilterUI();
        RefreshItemList();
    }


    private void ToggleCategory(string category)
    {
        EnsureDraftExists();

        ToggleValue(
            draftFilter.selectedCategories,
            category
        );
        if (draftFilter.selectedCategories.Count == 0)
        {
            draftFilter.selectedSubcategories.Clear();
            draftFilter.selectedProductIds.Clear();
        }

        RemoveInvalidSelectedItems();

        RefreshFilterUI();
        RefreshItemList();
    }


    private void ToggleSubcategory(string subcategory)
    {
        EnsureDraftExists();

        ToggleValue(
            draftFilter.selectedSubcategories,
            subcategory
        );

        RemoveInvalidSelectedItems();

        RefreshFilterUI();
        RefreshItemList();
    }


    public void ApplyFilter()
    {
        EnsureDraftExists();

        productManager.ApplyFilter(draftFilter);

        SetCanvasGroupVisible(
            filterPanelGroup,
            false
        );
    }


    public void ResetFilter()
    {
        EnsureDraftExists();

        draftFilter.Clear();

        productManager.ResetFilter();

        RefreshFilterUI();
        RefreshItemList();
    }


    public void ClosePanel()
    {
        // Discard temporary selections.
        draftFilter = null;

        SetCanvasGroupVisible(
            filterPanelGroup,
            false
        );
    }


    // --------------------------------
    // UI STATES
    // --------------------------------

    private void RefreshFilterUI()
    {
        if (draftFilter == null)
            return;


        bool categorySelected =
            draftFilter.selectedCategories.Count > 0;

        bool subcategorySelected =
            draftFilter.selectedSubcategories.Count > 0;


        // Category selected -> reveal subcategories.
        SetCanvasGroupVisible(
            subcategorySectionGroup,
            categorySelected
        );


        // Subcategory selected -> reveal item list.
        SetCanvasGroupVisible(
                itemsSectionGroup,
                categorySelected && subcategorySelected
        );


        // Category chip feedback.
        SetButtonState(
            watchesButton,
            draftFilter.selectedCategories.Contains("Watches")
        );

        SetButtonState(
            clothesButton,
            draftFilter.selectedCategories.Contains("Clothes")
        );

        SetButtonState(
            jewelleryButton,
            draftFilter.selectedCategories.Contains("Jewellery")
        );


        // Subcategory chip feedback.
        SetButtonState(
            maleButton,
            draftFilter.selectedSubcategories.Contains("Male")
        );

        SetButtonState(
            femaleButton,
            draftFilter.selectedSubcategories.Contains("Female")
        );

        SetButtonState(
            kidsBoyButton,
            draftFilter.selectedSubcategories.Contains("Kids-Boy")
        );

        SetButtonState(
            kidsGirlButton,
            draftFilter.selectedSubcategories.Contains("Kids-Girl")
        );
    }


    private void SetButtonState(
        Button button,
        bool selected)
    {
        button.image.color =
            selected ? selectedColor : normalColor;
    }


    private void SetCanvasGroupVisible(
        CanvasGroup group,
        bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }


    // --------------------------------
    // FILTER HELPERS
    // --------------------------------

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
        if (draftFilter == null)
        {
            draftFilter =
                productManager.AppliedFilter.Clone();
        }
    }


    private void RemoveInvalidSelectedItems()
    {
        for (
            int i = draftFilter.selectedProductIds.Count - 1;
            i >= 0;
            i--)
        {
            string selectedId =
                draftFilter.selectedProductIds[i];

            ProductData selectedProduct = null;


            foreach (
                ProductData product
                in productManager.allProducts)
            {
                if (product.productId == selectedId)
                {
                    selectedProduct = product;
                    break;
                }
            }


            if (selectedProduct == null)
            {
                draftFilter.selectedProductIds.RemoveAt(i);
                continue;
            }


            bool categoryMatch =
                draftFilter.selectedCategories.Count == 0 ||
                draftFilter.selectedCategories.Contains(
                    selectedProduct.category
                );


            bool subcategoryMatch =
                draftFilter.selectedSubcategories.Count == 0 ||
                draftFilter.selectedSubcategories.Contains(
                    selectedProduct.subcategory
                );


            if (!categoryMatch || !subcategoryMatch)
            {
                draftFilter.selectedProductIds.RemoveAt(i);
            }
        }
    }


    private void RefreshItemList()
    {
        // Clear previous filter item views
        for (int i = filterItemParent.childCount - 1; i >= 0; i--)
        {
            Destroy(filterItemParent.GetChild(i).gameObject);
        }

        if (draftFilter == null ||
            draftFilter.selectedCategories.Count == 0 ||
            draftFilter.selectedSubcategories.Count == 0)
        {
            return;
        }

        foreach (ProductData product in productManager.allProducts)
        {
            bool categoryMatch =
                draftFilter.selectedCategories.Contains(product.category);

            bool subcategoryMatch =
                draftFilter.selectedSubcategories.Contains(product.subcategory);

            if (!categoryMatch || !subcategoryMatch)
                continue;

            FilterItemView item =
                Instantiate(filterItemPrefab, filterItemParent);

            bool selected =
                draftFilter.selectedProductIds.Contains(product.productId);

            item.Setup(
                product,
                thumbnailCacheService,
                selected,
                OnItemSelectionChanged
            );
        }
    }

    private void OnItemSelectionChanged(
    string productId,
    bool selected)
    {
        if (selected)
        {
            if (!draftFilter.selectedProductIds.Contains(productId))
            {
                draftFilter.selectedProductIds.Add(productId);
            }
        }
        else
        {
            draftFilter.selectedProductIds.Remove(productId);
        }
    }
}