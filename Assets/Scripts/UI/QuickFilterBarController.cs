using UnityEngine;
using UnityEngine.UI;

public class QuickFilterBarController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField]
    private ProductManager productManager;

    [Header("Quick Filter Buttons")]
    [SerializeField]
    private Button watchesButton;

    [SerializeField]
    private Button clothesButton;

    [SerializeField]
    private Button jewelleryButton;

    [Header("Visuals")]
    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color selectedColor =
        new Color32(199, 132, 94, 255);


    private void Awake()
    {
        // We control the colors ourselves.
        // Unity's temporary Selected/Pressed states
        // should not overwrite them.
        DisableButtonTransitions();

        watchesButton.onClick.AddListener(
            () => ToggleQuickFilter("Watches")
        );

        clothesButton.onClick.AddListener(
            () => ToggleQuickFilter("Clothes")
        );

        jewelleryButton.onClick.AddListener(
            () => ToggleQuickFilter("Jewellery")
        );
    }


    private void OnEnable()
    {
        if (productManager != null)
        {
            productManager.ProductsChanged +=
                UpdateVisuals;
        }
    }


    private void Start()
    {
        UpdateVisuals();
    }


    private void OnDisable()
    {
        if (productManager != null)
        {
            productManager.ProductsChanged -=
                UpdateVisuals;
        }
    }


    private void ToggleQuickFilter(
        string category)
    {
        if (productManager == null)
            return;


        FilterState filter =
            productManager.AppliedFilter.Clone();


        // -------------------------------------
        // Toggle category
        // -------------------------------------

        if (
            filter.selectedCategories.Contains(
                category
            ))
        {
            filter.selectedCategories.Remove(
                category
            );
        }
        else
        {
            filter.selectedCategories.Add(
                category
            );
        }


        // -------------------------------------
        // Quick filters are CATEGORY filters.
        //
        // Remove detailed subcategory/item
        // restrictions so the user actually
        // sees the entire selected category.
        // -------------------------------------

        filter.selectedSubcategories.Clear();
        filter.selectedProductIds.Clear();


        productManager.ApplyFilter(
            filter
        );
    }


    private void UpdateVisuals()
    {
        if (productManager == null)
            return;


        FilterState filter =
            productManager.AppliedFilter;


        SetButtonVisual(
            watchesButton,
            filter.selectedCategories.Contains(
                "Watches"
            )
        );


        SetButtonVisual(
            clothesButton,
            filter.selectedCategories.Contains(
                "Clothes"
            )
        );


        SetButtonVisual(
            jewelleryButton,
            filter.selectedCategories.Contains(
                "Jewellery"
            )
        );
    }


    private void SetButtonVisual(
        Button button,
        bool selected)
    {
        if (button == null)
            return;


        if (button.targetGraphic != null)
        {
            button.targetGraphic.color =
                selected
                    ? selectedColor
                    : normalColor;
        }
    }


    private void DisableButtonTransitions()
    {
        if (watchesButton != null)
        {
            watchesButton.transition =
                Selectable.Transition.None;
        }

        if (clothesButton != null)
        {
            clothesButton.transition =
                Selectable.Transition.None;
        }

        if (jewelleryButton != null)
        {
            jewelleryButton.transition =
                Selectable.Transition.None;
        }
    }
}