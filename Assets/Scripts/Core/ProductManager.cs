using UnityEngine;
using System.Collections.Generic;

public class ProductManager : MonoBehaviour
{

    [SerializeField] private ProductDataLoader productDataLoader;

    public List<ProductData> allProducts = new List<ProductData>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(productDataLoader.LoadProducts(OnProductsLoaded));
    }

    void OnProductsLoaded(ProductCatalogJson catalog)
    {
        allProducts = catalog.products;
        Debug.Log("[MANAGER] Products loaded successfully. Total products: " + allProducts.Count);
    }

}
