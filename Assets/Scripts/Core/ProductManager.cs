using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ProductManager : MonoBehaviour
{
    [SerializeField] private ProductDataLoader productDataLoader;

    public List<ProductData> allProducts = new List<ProductData>();

    private List<ProductData> currentProducts =
        new List<ProductData>();

    public List<ProductData> CurrentProducts => currentProducts;

    public FilterState AppliedFilter { get; private set; }
        = new FilterState();

    public event Action ProductsChanged;
    [SerializeField]
    private ThumbnailCacheService thumbnailCacheService;


    void Awake()
    {
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
        StartCoroutine(
            productDataLoader.LoadProducts(
                OnProductsLoaded
            )
        );
    }


    private void OnProductsLoaded(ProductCatalogJson catalog)
    {
        allProducts = catalog.products;

        Debug.Log(
            "[MANAGER] Products loaded: " +
            allProducts.Count
        );

        StartCoroutine(
            PrepareInitialCatalogue()
        );
    }
    private IEnumerator PrepareInitialCatalogue()
    {
        // Preload thumbnails BEFORE catalogue cards appear.
        if (thumbnailCacheService != null)
        {
            yield return thumbnailCacheService.PrewarmProducts(
                allProducts,
                12
            );
        }

        // Only expose products after preload is finished.
        currentProducts =
            new List<ProductData>(allProducts);

        Debug.Log(
            "[MANAGER] Thumbnail prewarm finished."
        );

        ProductsChanged?.Invoke();
    }

    public void ApplyFilter(FilterState filter)
    {
        AppliedFilter = filter.Clone();

        currentProducts.Clear();

        foreach (ProductData product in allProducts)
        {
            if (MatchesFilter(product, AppliedFilter))
            {
                currentProducts.Add(product);
            }
        }

        Debug.Log(
            "[FILTER] Products found: "
            + currentProducts.Count
        );

        ProductsChanged?.Invoke();
    }


    public void ResetFilter()
    {
        AppliedFilter.Clear();

        currentProducts =
            new List<ProductData>(allProducts);

        ProductsChanged?.Invoke();
    }


    private bool MatchesFilter(
        ProductData product,
        FilterState filter)
    {
        bool categoryMatch =
            filter.selectedCategories.Count == 0 ||
            filter.selectedCategories.Contains(
                product.category
            );

        bool subcategoryMatch =
            filter.selectedSubcategories.Count == 0 ||
            filter.selectedSubcategories.Contains(
                product.subcategory
            );

        bool itemMatch =
            filter.selectedProductIds.Count == 0 ||
            filter.selectedProductIds.Contains(
                product.productId
            );

        return categoryMatch &&
               subcategoryMatch &&
               itemMatch;
    }
}