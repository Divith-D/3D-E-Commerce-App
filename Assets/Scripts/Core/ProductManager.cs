using System;
using System.Collections.Generic;
using UnityEngine;

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


    private void Start()
    {
        StartCoroutine(
            productDataLoader.LoadProducts(OnProductsLoaded)
        );
    }


    private void OnProductsLoaded(ProductCatalogJson catalog)
    {
        allProducts = catalog.products;

        currentProducts =
            new List<ProductData>(allProducts);

        Debug.Log(
            "[MANAGER] Products loaded: "
            + allProducts.Count
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